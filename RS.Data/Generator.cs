using Newtonsoft.Json;
using RS.Common;
using RS.Common.Extensions;
using RS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace RS.Data
{
    public class Generator
    {
        public SqlConnection Connection { get; private set; }
        public string BaseDmlFolderPath { get; private set; }

        public Generator(SqlConnection connection, string baseDmlFolderPath)
        {
            Connection = connection;
            BaseDmlFolderPath = baseDmlFolderPath;
        }

        public void GenerateDML(Action<string> output = null)
        {
            if (!Directory.Exists(BaseDmlFolderPath))
                Directory.CreateDirectory(BaseDmlFolderPath);

            if (Connection.State != System.Data.ConnectionState.Open)
                Connection.Open();

            if (output != null)
                output("Getting source database structure...");

            var schemas = new DDManager(Connection).GetSchemas(true).Where(el => el.Tables != null && el.Tables.Count > 0).ToList();

            foreach (var s in schemas)
            {
                if (s.Tables == null || s.Tables.Count == 0)
                    continue;

                var schemaOutputFolderPath = Path.Combine(BaseDmlFolderPath, s.Name);
                if (!Directory.Exists(schemaOutputFolderPath))
                    Directory.CreateDirectory(schemaOutputFolderPath);

                foreach (var t in s.Tables)
                {
                    var outputTableDmlFilePath = Path.Combine(schemaOutputFolderPath, t.Name + ".sql");
                    GenerateDML(t, outputTableDmlFilePath, output);
                }
            }

            if (output != null)
                output("Successfully generated DML.");
        }

        public void GenerateDML(Table table, string outputFilePath, Action<string> output = null)
        {
            DMLTableFileInfo dmlFileInfo = null;
            if (!ShouldUpdateTableDMLFile(table, outputFilePath, out dmlFileInfo))
                return;

            if (output != null)
                output("Generating DML for " + table.FullName + "...");

            if (File.Exists(outputFilePath))
                File.Delete(outputFilePath);

            var index = 0;
            const int setSize = 100;
            var commands = new List<Command>();
            do
            {
                commands = GetDMLCommands(table, index, setSize);
                if (commands != null && commands.Count > 0)
                    outputFilePath.AppendAllLines(commands.Select(el => el.Text));

                index += commands.Count;
            } while (commands.Count > 0 && commands.Count == setSize);

            var dmlInfoFilePath = outputFilePath.Replace(".sql", ".dmlinfo");
            dmlFileInfo.Generated = true;
            File.WriteAllText(dmlInfoFilePath, JsonConvert.SerializeObject(dmlFileInfo));
        }

        public void ApplyDML(Action<string> output = null)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
                Connection.Open();

            if (output != null)
                output("Getting destination database structure...");

            var schemas = new DDManager(Connection).GetSchemas(true).Where(el => el.Tables != null && el.Tables.Count > 0).ToList();

            foreach (var s in schemas)
            {
                if (s.Tables == null || s.Tables.Count == 0)
                    continue;

                foreach (var t in s.Tables)
                    ApplyDML(t, output);
            }

            if (output != null)
                output("Successfully applied DML.");
        }

        public void ApplyDML(Table table, Action<string> output = null)
        {
            var dmlFilePath = BaseDmlFolderPath.CombinePaths(table.SchemaName, table.Name + ".sql");
            if (!File.Exists(dmlFilePath))
            {
                if (output != null)
                    output("There is not DML file for table " + table.FullName);

                return;
            }

            if (output != null)
                output("Applying DML for table " + table.FullName + "...");

            using (var tran = TransactionHelper.CreateTransactionScope())
            {
                ExecuteCommand("ALTER TABLE " + table.FullName + " DISABLE TRIGGER all");
                ExecuteCommand("ALTER TABLE " + table.FullName + " NOCHECK CONSTRAINT ALL");

                var hasTableIdentityColumn = HasTableIdentityColumn(table);
                if (hasTableIdentityColumn)
                    ExecuteCommand("SET IDENTITY_INSERT " + table.FullName + " ON");

                const int maxCommands = 100;
                var index = -1;
                var noCommands = 0;
                var content = new StringBuilder();
                using (StreamReader reader = new StreamReader(dmlFilePath))
                {
                    while (!reader.EndOfStream)
                    {
                        index++;
                        var line = reader.ReadLine();
                        if (line.StartsWith("insert into ["))
                            noCommands++;

                        if (noCommands == maxCommands + 1)
                        {
                            ExecuteCommand(content.ToString());
                            noCommands = 1;
                            content = new StringBuilder();
                            GC.Collect();
                        }

                        content.AppendLine(line);
                    }
                }

                ExecuteCommand(content.ToString());

                if (hasTableIdentityColumn)
                    ExecuteCommand("SET IDENTITY_INSERT " + table.FullName + " OFF");
                ExecuteCommand("ALTER TABLE " + table.FullName + " CHECK CONSTRAINT ALL");
                ExecuteCommand("ALTER TABLE " + table.FullName + " ENABLE TRIGGER all");

                tran.Complete();
            }
        }

        private void ExecuteCommand(string text)
        {
            if (text.IsNullOrWhiteSpace())
                return;

            text = text.Replace(((char)13).ToString() + ((char)10).ToString(), string.Empty);

            var command = Connection.CreateCommand();
            command.CommandText = text;
            command.ExecuteNonQuery();
        }

        private bool HasTableIdentityColumn(Table table)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "select * from sys.objects o inner join sys.columns c on o.object_id = c.object_id where type_desc = 'USER_TABLE' and c.is_identity = 1 and SCHEMA_NAME(o.schema_id) = '" + table.SchemaName + "' and o.name = '" + table.Name + "'";
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                    return true;
            }

            return false;
        }

        private bool ShouldUpdateTableDMLFile(Table table, string filePath, out DMLTableFileInfo dmlFileInfo)
        {
            dmlFileInfo = new DMLTableFileInfo()
            {
                SchemaName = table.SchemaName,
                TableName = table.Name
            };

            try
            {
                dmlFileInfo.DataHash = GetTableDataHash(table);

                var dmlInfoFilePath = filePath.Replace(".sql", ".dmlinfo");
                if (!File.Exists(dmlInfoFilePath))
                    return true;

                string line = null;
                using (StreamReader reader = new StreamReader(dmlInfoFilePath))
                {
                    line = reader.ReadLine();
                }

                if (line.IsNullOrWhiteSpace())
                    return true;

                try
                {
                    var existingDmlFileInfo = JsonConvert.DeserializeObject<DMLTableFileInfo>(line);
                    if (existingDmlFileInfo == null || !dmlFileInfo.Generated)
                        return true;

                    if (!existingDmlFileInfo.DataHash.Equals(dmlFileInfo.DataHash))
                        return true;
                }
                catch { return true; }
            }
            catch { return true; }

            return false;
        }

        public string GetTableDataHash(Table table)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "select CHECKSUM_AGG(CHECKSUM (*)) as Hash from " + table.FullName;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                    return DataConverter.ToString(reader["Hash"]);
            }

            return null;
        }

        private bool ShouldIgnoreColumnWhenCreatingDML(Column c)
        {
            if (c == null)
                return true;

            if (c.DataType == System.Data.SqlDbType.Timestamp)
                return true;

            if (c.IsComputed)
                return true;

            return false;
        }

        private List<Command> GetDMLCommands(Table table, int index = 0, int setSize = 100)
        {
            if (table == null || table.Columns == null || table.Columns.Count == 0)
                return null;

            var commands = new List<Command>();

            var query = "'insert into " + table.FullName + " (";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var c = table.Columns[i];
                if (ShouldIgnoreColumnWhenCreatingDML(c))
                    continue;

                query += "[" + c.Name + "]";

                if (i < table.Columns.Count - 1)
                    query += ", ";
            }

            query += ") values (";

            for (int i = 0; i < table.Columns.Count; i++)
            {
                var c = table.Columns[i];

                if (ShouldIgnoreColumnWhenCreatingDML(c))
                    continue;

                var hasQuotes = c.DataType == System.Data.SqlDbType.Char || c.DataType == System.Data.SqlDbType.Date || c.DataType == System.Data.SqlDbType.DateTime || c.DataType == System.Data.SqlDbType.DateTime2 || c.DataType == System.Data.SqlDbType.DateTimeOffset || c.DataType == System.Data.SqlDbType.NChar || c.DataType == System.Data.SqlDbType.NText || c.DataType == System.Data.SqlDbType.NVarChar || c.DataType == System.Data.SqlDbType.SmallDateTime || c.DataType == System.Data.SqlDbType.Text || c.DataType == System.Data.SqlDbType.Time || c.DataType == System.Data.SqlDbType.Timestamp || c.DataType == System.Data.SqlDbType.UniqueIdentifier || c.DataType == System.Data.SqlDbType.VarChar || c.DataType == System.Data.SqlDbType.Variant || c.DataType == System.Data.SqlDbType.Xml;

                query += "' + (case when [" + c.Name + "] is null then 'null' else ";

                if (hasQuotes)
                    query += "'''' + ";

                query += "'{" + i.ToString() + "}'";

                if (hasQuotes)
                    query += "+ ''''";

                query += " end) + '";

                if (i < table.Columns.Count - 1)
                    query += ", ";
            }

            query += ")'";

            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select");
            commandBuilder.AppendLine(query);
            commandBuilder.AppendLine(", *");
            commandBuilder.AppendLine("from");
            commandBuilder.AppendLine("     " + table.FullName);
            commandBuilder.AppendLine("order by [" + table.Columns[0].Name + "]");
            commandBuilder.AppendLine(" offset " + index + " rows");
            if (setSize > 0)
                commandBuilder.AppendLine(" fetch next " + setSize + " rows only");

            var command = Connection.CreateCommand();
            command.CommandText = commandBuilder.ToString();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var line = reader[0].ToString();

                    var commandToExecute = new Command(CommandTypes.Insert, line, table);
                    if (table.Columns.Count > 0)
                    {
                        var parameters = new object[table.Columns.Count];
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            var c = table.Columns[i];
                            if (ShouldIgnoreColumnWhenCreatingDML(c))
                                continue;

                            object value = null;

                            if (c.DataType == System.Data.SqlDbType.VarBinary || c.DataType == System.Data.SqlDbType.Binary)
                            {
                                var bytes = DataConverter.ToByteArray(reader[i + 1]);
                                if (bytes == null || bytes.LongLength == 0)
                                {
                                    value = "null";
                                }
                                else
                                {
                                    value = "0x" + BitConverter.ToString(bytes).Replace("-", "");

                                    GC.AddMemoryPressure(bytes.LongLength);
                                }
                            }
                            else if (c.DataType == System.Data.SqlDbType.Bit)
                            {
                                value = DataConverter.ToBoolean(reader[i + 1]) ? "1" : "0";
                            }
                            else value = DataConverter.ToString(reader[i + 1]).Replace("'", "''");

                            parameters[i] = value.ToString();
                        }

                        commandToExecute.Text = string.Format(commandToExecute.Text, parameters);
                    }

                    commands.Add(commandToExecute);
                }
            }

            GC.Collect();

            return commands;
        }
    }
}
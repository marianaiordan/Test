using RS.Common;
using RS.Common.Extensions;
using RS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace RS.Data
{
    public class DDManager
    {
        public SqlConnection Connection { get; private set; }

        public DDManager(SqlConnection connection)
        {
            Connection = connection;
        }

        public List<Schema> GetSchemas(bool getCompleteInfo = false)
        {
            var schemas = new List<Schema>();

            var command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM INFORMATION_SCHEMA.SCHEMATA order by SCHEMA_NAME";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    schemas.Add(new Schema()
                    {
                        Name = DataConverter.ToString(reader["SCHEMA_NAME"])
                    });
                }
            }

            if (!getCompleteInfo || schemas.Count == 0)
                return schemas;

            List<Table> tables = GetTables();
            List<Column> columns = GetColumns();
            List<ForeignKey> foreignKeys = GetForeignKeys();

            foreach (var s in schemas)
            {
                foreach (var t in tables.Where(el => el.SchemaName == s.Name))
                {
                    t.Columns = columns.Where(el => el.TableName == t.Name && el.TableSchemaName == s.Name).OrderBy(el => el.OrdinalPosition).ToList();

                    if (foreignKeys != null)
                        foreach (var c in t.Columns)
                            c.ForeignKey = foreignKeys.FirstOrDefault(el => el.FK_Schema == c.TableSchemaName && el.FK_Table == c.TableName && el.FK_Column == c.Name);

                    if (s.Tables == null)
                        s.Tables = new List<Table>();

                    s.Tables.Add(t);
                }
            }

            return schemas;
        }

        public List<Table> GetTables(string schema = null)
        {
            var tables = new List<Table>();

            var command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";
            if (!schema.IsNullOrWhiteSpace())
                command.CommandText += " and TABLE_SCHEMA = '" + schema + "'";
            command.CommandText += " order by TABLE_NAME";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tables.Add(new Table()
                    {
                        Name = DataConverter.ToString(reader["TABLE_NAME"]),
                        SchemaName = DataConverter.ToString(reader["TABLE_SCHEMA"])
                    });
                }
            }

            return tables;
        }

        public List<Column> GetColumns(string table = null)
        {
            var columns = new List<Column>();

            var command = Connection.CreateCommand();
            command.CommandText = "SELECT *, case when exists(select * from sys.computed_columns cc where cc.object_id = OBJECT_ID('[' + c.TABLE_SCHEMA + '].[' + c.TABLE_NAME + ']') and cc.name = c.COLUMN_NAME) then 1 else 0 end as IsComputed FROM INFORMATION_SCHEMA.COLUMNS c";
            if (!table.IsNullOrWhiteSpace())
                command.CommandText += " where TABLE_NAME = '" + table + "'";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(new Column()
                    {
                        Name = DataConverter.ToString(reader["COLUMN_NAME"]),
                        DataType = TypeConvertor.Parse(reader["DATA_TYPE"].ToString()),
                        Default = DataConverter.ToString(reader["COLUMN_DEFAULT"]),
                        IsNullable = DataConverter.ToString(reader["IS_NULLABLE"]) == "YES",
                        MaximumLength = DataConverter.ToNullableInt32(reader["CHARACTER_MAXIMUM_LENGTH"]),
                        Precision = DataConverter.ToNullableInt32(reader["NUMERIC_PRECISION"]),
                        Scale = DataConverter.ToNullableInt32(reader["NUMERIC_SCALE"]),
                        TableName = DataConverter.ToString(reader["TABLE_NAME"]),
                        TableSchemaName = DataConverter.ToString(reader["TABLE_SCHEMA"]),
                        OrdinalPosition = DataConverter.ToInt32(reader["ORDINAL_POSITION"]),
                        IsComputed = DataConverter.ToBoolean(reader["IsComputed"])
                    });
                }
            }

            return columns;
        }

        public List<ForeignKey> GetForeignKeys()
        {
            var keys = new List<ForeignKey>();
            var command = Connection.CreateCommand();
            command.CommandText = @"SELECT
	FK_Schema = FK.TABLE_SCHEMA,
    FK_Table = FK.TABLE_NAME,
    FK_Column = CU.COLUMN_NAME,
	PK_Schema = PK.TABLE_SCHEMA,
    PK_Table = PK.TABLE_NAME,
    PK_Column = PT.COLUMN_NAME,
    Constraint_Name = C.CONSTRAINT_NAME
FROM
    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK
    ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK
    ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU
    ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
INNER JOIN (
            SELECT
                i1.TABLE_NAME,
                i2.COLUMN_NAME
            FROM
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2
                ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
            WHERE
                i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
           ) PT
    ON PT.TABLE_NAME = PK.TABLE_NAME";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var k = new ForeignKey();
                    k.Name = DataConverter.ToString(reader["CONSTRAINT_NAME"]);
                    k.FK_Schema = DataConverter.ToString(reader["FK_Schema"]);
                    k.FK_Table = DataConverter.ToString(reader["FK_Table"]);
                    k.FK_Column = DataConverter.ToString(reader["FK_Column"]);
                    k.PK_Schema = DataConverter.ToString(reader["PK_Schema"]);
                    k.PK_Table = DataConverter.ToString(reader["PK_Table"]);
                    k.PK_Column = DataConverter.ToString(reader["PK_Column"]);

                    keys.Add(k);
                }
            }

            return keys;
        }
    }
}
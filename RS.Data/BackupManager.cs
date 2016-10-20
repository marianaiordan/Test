using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Data
{
    public class BackupManager
    {
        public SqlConnector Connector { get; private set; }

        public BackupManager(SqlConnector sqlConnector)
        {
            Connector = sqlConnector;
        }

        public string GetBackupPath()
        {
            var query = @"declare @BackupDirectory varchar(1000) exec master..xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'BackupDirectory', @BackupDirectory output; select @BackupDirectory";

            return Connector.RunQueryFirstValue(query);
        }

        public void Backup(string databaseName, string backupFilePath)
        {
            var query = string.Format(@"BACKUP DATABASE [{0}] TO DISK = '{1}' WITH FORMAT", databaseName, backupFilePath);

            Connector.RunQuery(query);
        }
    }
}
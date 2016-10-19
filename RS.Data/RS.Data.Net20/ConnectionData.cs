using System;
using System.Data.SqlClient;

namespace RS.Data
{
    public class ConnectionData
    {
        public string Instance { get; set; }

        public string Database { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public ConnectionData()
        {
        }

        public ConnectionData(string instance, string database, string username = null, string password = null)
        {
            Instance = instance;
            Database = database;
            Username = username;
            Password = password;
        }

        public ConnectionData(string connectionString)
        {
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString);

            Instance = connectionBuilder.DataSource;
            Database = connectionBuilder.InitialCatalog;
            Username = connectionBuilder.UserID;
            Password = connectionBuilder.Password;
        }

        public ConnectionData Clone()
        {
            return new ConnectionData()
            {
                Database = Database,
                Instance = Instance,
                Password = Password,
                Username = Username
            };
        }

        public string GenerateConnectionString(bool includeDatabase = true, string provider = null)
        {
            switch (provider)
            {
                case Constants.OledbProviders.ORACLE:
                    return string.Format(RS.Common.Constants.ConnectionStringOracleTemplate, Instance, Username, Password);
                case Constants.OledbProviders.MSQL:
                default:
                    return string.Format(RS.Common.Constants.ConnectionStringSimpleTemplate, Instance, includeDatabase ? Database : null, Username, Password);
            }
        }
    }
}
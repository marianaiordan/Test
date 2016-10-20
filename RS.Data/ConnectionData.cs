using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using RS.Common.Extensions;

namespace RS.Data
{
    [DataContract]
    [Serializable]
    public class ConnectionData
    {
        [DataMember]
        public string Instance { get; set; }

        [DataMember]
        public string Database { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        public ConnectionData()
        {
        }

        public ConnectionData(string instance, string database, string username = null, string password = null)
        {
            Instance = instance;
            Database = database;
            Username = username
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

        public string GenerateEFConnectionString(string baseConnection)
        {
            if (baseConnection.IsNullOrWhiteSpace())
                return baseConnection;

            var entityBuilder = new EntityConnectionStringBuilder(baseConnection);
            var sqlBuilder = new SqlConnectionStringBuilder(entityBuilder.ProviderConnectionString);
            sqlBuilder.DataSource = Instance;
            sqlBuilder.InitialCatalog = Database;
            sqlBuilder.UserID = Username;
            sqlBuilder.Password = Password;

            entityBuilder.ProviderConnectionString = sqlBuilder.ToString();

            var efConnectionString = entityBuilder.ToString();

            return efConnectionString;
        }
    }
}

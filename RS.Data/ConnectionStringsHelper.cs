using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace RS.Data
{
    public class ConnectionStringsHelper
    {
        public static string GenerateFromEFConnectionString(string connectionName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            string providerConnectionString = new EntityConnectionStringBuilder(connectionString).ProviderConnectionString;
            var connectionBuilder = new SqlConnectionStringBuilder(providerConnectionString);

            return connectionBuilder.ToString();
        }
    }
}
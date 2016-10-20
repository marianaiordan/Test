using RS.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace RS.Data
{
    public class SqlConnector
    {
        public ConnectionData ConnectionData { get; private set; }
        public SqlConnector(ConnectionData connectionData)
        {
            ConnectionData = connectionData;
        }

        public void RunQuery(string query)
        {
            using (var connection = new SqlConnection(ConnectionData.GenerateConnectionString()))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        public object RunQueryScalar(string query)
        {
            using (var connection = new SqlConnection(ConnectionData.GenerateConnectionString()))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = query;
                return command.ExecuteScalar();
            }
        }

        public string RunQueryFirstValue(string query)
        {
            string result = null;
            RunQuery(query, (reader) =>
            {
                if (reader.FieldCount == 0)
                    return;

                result = DataConverter.ToString(reader[0]);
            });

            return result;
        }

        public void RunQuery(string query, Action<SqlDataReader> readerAction)
        {
            using (var connection = new SqlConnection(ConnectionData.GenerateConnectionString()))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    if (readerAction == null)
                        return;

                    while (reader.Read())
                    {
                        readerAction(reader);
                    }
                }
            }
        }
    }
}
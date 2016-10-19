using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using RS.Common.Extensions;

namespace RS.Data.Extensions
{
    public static class SqlCommandExtensions
    {
        private static bool IsGo(string psCommandLine)
        {
            if (psCommandLine == null)
                return false;

            psCommandLine = psCommandLine.Trim();
            if (string.Compare(psCommandLine, "GO", StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            if (psCommandLine.StartsWith("GO", StringComparison.OrdinalIgnoreCase))
            {
                psCommandLine = (psCommandLine + "--").Substring(2).Trim();
                if (psCommandLine.StartsWith("--"))
                    return true;
            }

            return false;
        }

        public static void ExecuteNonQueryWithGos(this SqlCommand poSqlCommand)
        {
            string sCommandLong = poSqlCommand.CommandText;

            using (StringReader oStringReader = new StringReader(sCommandLong))
            {
                string sCommandLine;
                string sCommandShort = string.Empty;
                while ((sCommandLine = oStringReader.ReadLine()) != null)
                    if (IsGo(sCommandLine))
                    {
                        if (!sCommandShort.IsNullOrWhiteSpace())
                        {
                            if ((poSqlCommand.Connection.State & ConnectionState.Open) == 0)
                                poSqlCommand.Connection.Open();
                            using (SqlCommand oSqlCommand = new SqlCommand(sCommandShort, poSqlCommand.Connection))
                            {
                                oSqlCommand.ExecuteNonQuery();
                            }
                        }
                        sCommandShort = string.Empty;
                    }
                    else
                        sCommandShort += sCommandLine + "\r\n";
            }
        }
    }
}
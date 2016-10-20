using System;
using System.Collections.Generic;
using System.Text;

namespace RS.Data
{
    public static class Constants
    {
        public static readonly DateTime MinSqlDate = new DateTime(1753, 1, 1);
        public static class ErrorCodes
        {
            public const int SQL_DEADLOCK_ERROR_CODE = 1205;
            public const int SQL_TIMEOUT_ERROR_CODE = -2;
        }

        public static class OledbProviders
        {
            public const string ORACLE = "OraOLEDB.Oracle";
            public const string MSQL = "SQLOLEDB";
        }
    }
}
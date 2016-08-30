using System;
using System.Collections.Generic;
using System.Text;

namespace ContactRoute.Shared
{
    public class DatabaseConnectionInfo
    {
        public string ConnectionString = "";
        public string ConnectionType = "";
        public bool UseConnection = true;
        public bool MultiTableMode = false;
        public int ConnectionTimeOut = 3600;
        public bool Update = true;

        public DatabaseConnectionInfo(string connectionString, string type, bool use, bool multiTable, int connectionTimeOut, bool update)
        {
            ConnectionString = connectionString;
            ConnectionType = type;
            UseConnection = use;
            MultiTableMode = multiTable;
            ConnectionTimeOut = connectionTimeOut;
            Update = update;
        }

        public override string ToString()
        {
            return string.Format("Type is {0}, need to be updated {1}, using timeout {2}", ConnectionType.ToString(), Update.ToString(), ConnectionTimeOut.ToString());
        }
    }
}

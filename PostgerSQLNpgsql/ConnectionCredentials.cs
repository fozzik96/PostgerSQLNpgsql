using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;


namespace PostgerSQLNpgsql
{
    class ConnectionCredentials
    {

        public string ConnectionStr { get; set; }
        
        public string serverName { get; set; }

        public string dbName { get; set; }

    }
}

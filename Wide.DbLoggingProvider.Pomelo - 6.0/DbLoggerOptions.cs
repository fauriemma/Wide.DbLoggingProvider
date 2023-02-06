using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wide82.DbLoggingProvider
{
    public class DbLoggerOptions
    {
        public string ConnectionString { get; set; }

        public string LogPath { get; set; }

        public bool AsyncWrite { get; set; }

        public string ApplicationName { get; set; }

        public DbLoggerOptions()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data.Database;

namespace Nop.Web.Models.Install
{
    public class ConnectionStringModel : INopConnectionString
    {
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }

        public bool IntegratedSecurity { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }
    }
}

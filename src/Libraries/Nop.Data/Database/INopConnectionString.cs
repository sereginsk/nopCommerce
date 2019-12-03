﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Database
{
    public interface INopConnectionString
    {
        string DatabaseName { get; set; }
        string ServerName { get; set; }

        bool IntegratedSecurity { get; set; }

        string Username { get; set; }
        string Password { get; set; }
    }
}

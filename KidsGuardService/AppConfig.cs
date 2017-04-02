using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsKidsGuardService
{
    public static class AppConfig
    {
        public static int sessionTimeout = 1; // 30 minutes
        public static int breakTime = 1; // 5 minutes
        public static int gmailTime = 20;  // 20 minutes, read from log when startup and update to log every 1 mintue
        public static string userToMonitor = "bohan";  // list of user be monitored

        // application setting

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KidsComputerGuard
{
    public static class AppConfig
    {
        public static int sessionTimeout = 30; // 30 minutes
        public static int breakTime = 5; // 5 minutes
        public static int gmailTime = 20;  // 20 minutes, read from log when startup and update to log every 1 mintue

        public static string userToMonitor = "bohan";  // list of user be monitored
        public static string processExcluded = "explorer;KidsComputerGuard";  // list of process to be ignored
        public static string titleNotAllowed = "Agar.io;tanki";  // list of user be monitored

        // application setting
        public static int updateStatInterval = 10; // update usage statistics interval, default to 10 seconds
        public static int reminderInterval = 15;  // station lock checking interval, 30 seconds
        public static int saveInterval = 60;  // save stat to db interval, 60 seconds

        public static bool isProcessExcluded(string process)
        {
            if (String.IsNullOrEmpty(process))
                return true;

            string[] words = processExcluded.Split(';');
            foreach (string word in words)
            {
                if (process.Equals(word, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool isTitleAllowed(string title)
        {
            if (String.IsNullOrEmpty(title))
                return true;

            string[] words = titleNotAllowed.Split(';');
            foreach (string word in words)
            {
                if (title.IndexOf(word, StringComparison.OrdinalIgnoreCase)>=0)
                    return false;
            }

            return true;
        }
    }
}

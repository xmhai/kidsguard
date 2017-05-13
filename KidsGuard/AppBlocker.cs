using KidsComputerGuard;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KidsComputerGuard
{
    public class AppBlocker
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AppBlocker));

        private int _totalAllowedTime = 3600;
        private Dictionary<string, int> _appTimeDict = new Dictionary<string, int>();
        private UsageStat _usageStat;

        public AppBlocker(UsageStat usageStat)
        {
            _usageStat = usageStat;
            _totalAllowedTime = KidsGuardConfig.GetConfig().TotalAllowedTime;
            initAppTimeDict();
        }

        private void initAppTimeDict()
        {
            foreach(KidsGuardConfig.MonitoredApp blockedApp in KidsGuardConfig.GetConfig().MonitoredApps)
            {
                int allowedTime = blockedApp.AllowedTime;
                allowedTime = allowedTime - getAppTimeUsed(blockedApp.Title); // deduct today usage time
                _appTimeDict.Add(blockedApp.Title, allowedTime);
            }
        }

        private int getAppTimeUsed(string blockedAppTitle)
        {
            int timeUsed = 0;

            foreach (KeyValuePair<string, int> programTime in _usageStat.programTime)
            {
                if (programTime.Key.IndexOf(blockedAppTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    timeUsed += programTime.Value;
                }
            }

            return timeUsed;
        }

        public void process(string processName, string title, int activeTime)
        {
            if (processName==null || title==null)
            {
                return;
            }

            string key = String.Empty;
            foreach (KeyValuePair<string, int> appTime in _appTimeDict)
            {
                if (title.IndexOf(appTime.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // it is the application need to check allowedTime
                    key = appTime.Key;
                    break;
                }
            }

            if (!key.Equals(String.Empty))
            {
                _totalAllowedTime -= activeTime;
                _appTimeDict[key] -= activeTime;
                if (_appTimeDict[key] <= 0 || _totalAllowedTime <=0)
                {
                    logger.Info("Application '"+ title + "' is forced to close due to exceed allowedTime!");

                    // application timeout, close the application
                    Win32.KillActiveProcess();
                }
            }
        }
    }
}

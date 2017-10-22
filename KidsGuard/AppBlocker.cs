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
                // configured time
                int allowedTime = blockedApp.AllowedTime;
                string blockedAppTitle = blockedApp.Title.ToLower();

                // for weekday, only play for 5 minutes
                //DateTime dt = new DateTime();
                //if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday)
                //{
                //    allowedTime = 300; // 5 minutes
                //}

                // deduct today used time
                allowedTime = allowedTime - _usageStat.getAppTimeUsed(blockedAppTitle);

                logger.Info(blockedAppTitle + " initial allow time: " + allowedTime);
                _appTimeDict.Add(blockedAppTitle, allowedTime);
            }
        }

        public void process(string processName, string title, int activeTime)
        {
            if (processName==null || title==null)
            {
                return;
            }

            title = title.ToLower();
            foreach (KeyValuePair<string, int> appTime in _appTimeDict)
            {
                String key = appTime.Key;
                if (title.IndexOf(key) >= 0)
                {
                    // it is the application need to check allowedTime
                    _totalAllowedTime -= activeTime;
                    _appTimeDict[key] -= activeTime;
                    logger.Info(key + " allow time left: " + _appTimeDict[key]);

                    if (_appTimeDict[key] <= 0 || _totalAllowedTime <= 0)
                    {
                        logger.Info("Application '" + title + "' is forced to close due to exceed allowedTime!");

                        // application timeout, close the application
                        Win32.KillActiveProcess();
                    }

                    break;
                }
            }
        }
    }
}

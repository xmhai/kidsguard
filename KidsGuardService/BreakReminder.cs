using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;
using log4net.Config;

namespace WindowsKidsGuardService
{
    class BreakReminder
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BreakReminder));

        private DateTime startUpTime = DateTime.Now;
        private DateTime sessionStartTime = DateTime.Now;
        private DateTime lockStartTime = DateTime.Now;
        private string state = "RUN";

        public bool checkBreakTime()
        {
            // if working for 60 minutes, take a break for 10 minutes
            if (state == "RUN")
            {
                // update UI
                TimeSpan systemRunningTime = ((TimeSpan)(DateTime.Now - startUpTime));
                TimeSpan sessionRunningTime = ((TimeSpan)(DateTime.Now - sessionStartTime));

                if (sessionRunningTime.TotalMinutes > AppConfig.sessionTimeout)
                {
                    logger.Info("Lock Station");

                    // lock computer
                    state = "LOCK";
                    lockStartTime = DateTime.Now;
                    Win32.LockWorkStation();
                    return true;
                }
            }

            if (state == "LOCK")
            {
                if (((TimeSpan)(DateTime.Now - lockStartTime)).TotalMinutes > AppConfig.breakTime) {
                    logger.Info("Lock Period Ended");

                    state = "RUN";
                    sessionStartTime = DateTime.Now;
                }
                else // unlock during locking period, lock again
                {
                    logger.Info("Within Lock Period, relock station!!!");

                    Win32.LockWorkStation();
                }
            }

            return false;
        }
    }
}

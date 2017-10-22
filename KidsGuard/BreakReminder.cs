using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using log4net;
using log4net.Config;

namespace KidsComputerGuard
{
    [Serializable()]
    class BreakReminder
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BreakReminder));

        public void checkBreakTime(UsageStat usageStat)
        {
            if (usageStat.State == "RUN")
            {
                //if (AppConfig.userToMonitor.IndexOf(Environment.UserName) == -1)
                //{
                //    return;
                //}

                // first reminder (10 mins)
                if (usageStat.SessionTime == 10 * 60 && !usageStat.Reminder1)
                {
                    usageStat.Reminder1 = true;

                    new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                    {
                        System.Windows.Forms.MessageBox.Show(null, "Beware the power of the dark side!", "KidsGuard", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    })).Start();
                }

                // second reminder (5 mins)
                if (usageStat.SessionTime == 5 * 60 && !usageStat.Reminder2)
                {
                    usageStat.Reminder2 = true;

                    new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                    {
                        System.Windows.Forms.MessageBox.Show(null, "The Emperor is most displeased with your lack of apparent progress!", "KidsGuard", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    })).Start();
                }

                if (usageStat.SessionTime == 0)
                {
                    logger.Info("Lock Station due to session timeout");

                    usageStat.UpdateToLock();

                    // lock computer
                    Win32.LockWorkStation();
                }

                // check total computer time
                if (usageStat.getTotalComputerTime() >= KidsGuardConfig.GetConfig().TotalComputerTime)
                {
                    logger.Info("Lock Station due to total computer time is used up");

                    usageStat.UpdateToLock();

                    // lock computer
                    Win32.LockWorkStation();
                }
            }
        }
    }
}

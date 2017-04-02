using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Management;
using System.Configuration;

namespace WindowsKidsGuardService
{
    public partial class KidsGuardService : ServiceBase
    {
        private string _monitorUserName = String.Empty;
        private string _currentUserName = String.Empty;

        public KidsGuardService()
        {
            InitializeComponent();

            eventLog1 = new System.Diagnostics.EventLog();
            //if (!System.Diagnostics.EventLog.SourceExists("KidsGuardServiceLog"))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource("KidsGuardServiceLog", null);
            //}
            eventLog1.Source = "KidsGuardService";
            //eventLog1.Log = "KidsGuardServiceLog";

            string monitorUserName = ConfigurationSettings.AppSettings["UserName"];
            _monitorUserName = monitorUserName.ToUpper();
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            Win32.ServiceStatus serviceStatus = new Win32.ServiceStatus();
            serviceStatus.dwCurrentState = Win32.ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            Win32.SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            string username = getLoginUserName();
            eventLog1.WriteEntry("KidsGuardService Start, current login user name: "+ username + ", monitor user name: "+ _monitorUserName, EventLogEntryType.Information);

            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 5000; // 60 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Enabled = true;
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = Win32.ServiceState.SERVICE_RUNNING;
            Win32.SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        private string getLoginUserName()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            string username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
            return username.ToUpper();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            string username = getLoginUserName();
            if (!_currentUserName.Equals(username))
            {
                eventLog1.WriteEntry("Login user changed, new user: " + username, EventLogEntryType.Information);
                _currentUserName = username;
            }

            // not login yet
            if (String.IsNullOrEmpty(username))
            {
                return;
            }

            // not the user expected
            if (username.IndexOf(_monitorUserName)==-1)
            {
                return;
            }

            if (!isKidsGuardRunning(username))
            {
                eventLog1.WriteEntry("Current Login User: " + username, EventLogEntryType.Information);

                // launch the application
                string applicationName = ConfigurationSettings.AppSettings["KidsGuard"];
                eventLog1.WriteEntry("KidsGuard is not running, start it: "+ applicationName, EventLogEntryType.Information);
                ApplicationLoader.PROCESS_INFORMATION procInfo;
                ApplicationLoader.StartProcessAndBypassUAC(applicationName, username, eventLog1, out procInfo);
            }
        }

        bool isKidsGuardRunning(string loginUser)
        {
            Process[] processes = Process.GetProcessesByName("KidsComputerGuard");
            foreach (Process p in processes)
            {
                string owner = ApplicationLoader.GetProcessOwner(p.Id);
                if (owner.Equals(loginUser, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("KidsGuardService Stop.");
        }

        protected override void OnContinue()
        {
        }  
    }
}

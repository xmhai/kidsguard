using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KidsComputerGuard
{
    [Serializable()]
    public class UsageStat
    {
        // system statistics
        public DateTime StartUpTime = DateTime.Now;

        public Dictionary<string, int> programTime = new Dictionary<string, int>();
        public Dictionary<string, int> urlTime = new Dictionary<string, int>();
        public Dictionary<string, int> siteTime = new Dictionary<string, int>();

        // break reminder state
        public int SessionTime = AppConfig.sessionTimeout * 60;
        public DateTime LockStartTime = DateTime.Now.AddDays(-1);

        public string State = "RUN";

        public bool Reminder1 = false;
        public bool Reminder2 = false;

        public void UpdateToLock()
        {
            State = "LOCK";
            LockStartTime = DateTime.Now;
        }

        public void RestartSession()
        {
            State = "RUN";
            SessionTime = AppConfig.sessionTimeout * 60;

            Reminder1 = false;
            Reminder2 = false;
        }

        public void UpdateSessionTime(int timeUsed)
        {
            SessionTime = SessionTime - timeUsed;

            if (SessionTime < 0)
                SessionTime = 0;
        }

        public TimeSpan SystemRunningTime
        {
            get { return ((TimeSpan)(DateTime.Now - StartUpTime)); }
        }

        // add program usage time in seconds
        public int addProgramTime(string processName, string title, int time)
        {
            String program = processName + ":" + title;
            return addTime(programTime, program, time);
        }

        // add url usage time in seconds
        public void addUrlTime(string url, int time)
        {
            addTime(urlTime, url, time);

            // add site time
            //string site = url.IndexOf("/");

            // add gmail time
            if (url.IndexOf("mail.google.com") != -1)
            {
                //gmailTime = gmailTime + time;
            }
        }

        private int addTime(Dictionary<string, int> dict, string key, int time)
        {
            if (dict.ContainsKey(key))
            {
                int value = dict[key];
                value = value + time;
                dict[key] = value;
            }
            else
            {
                dict[key] = time;
            }
            return dict[key];
        }
    }
}

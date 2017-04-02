using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsKidsGuardService
{
    class UsageStat
    {
        public Dictionary<string, int> programTime = new Dictionary<string, int>();
        public Dictionary<string, int> urlTime = new Dictionary<string, int>();

        // add program usage time in seconds
        public void addProgramTime(string program, int time)
        {
            addTime(urlTime, program, time);
        }

        // add url usage time in seconds
        public void addUrlTime(string url, int time)
        {
            addTime(urlTime, url, time);
        }

        private void addTime(Dictionary<string, int> dict, string key, int time)
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
        }

        public void saveToDisk()
        {
        }
    }
}

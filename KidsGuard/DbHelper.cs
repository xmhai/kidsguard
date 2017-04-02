using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.SQLite;

using log4net;

namespace KidsComputerGuard
{
    class DbHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DbHelper));

        static SQLiteConnection conn;

        // Static constructor is called at most one time, before any
        // instance constructor is invoked or member is accessed.
        static DbHelper()
        {
            connectDb();
        }

        private static void connectDb()
        {
            string dbFileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\kidsguard.db";
            String connectionString = "Data Source=" + dbFileName + ";Version=3;";
            conn = new SQLiteConnection(connectionString);
            conn.Open();
        }

        public static string getConfigValue(string key)
        {
            string val = null;
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                SQLiteParameter p1 = new SQLiteParameter("sysKey", key);
                cmd.CommandText = @"SELECT SYS_VAL FROM SYS_CFG WHERE SYS_KEY=@sysKey";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(p1);
                SQLiteDataReader r = cmd.ExecuteReader();
                r.Read();
                val = (string)r["SYS_VAL"];
                r.Close();
            }
            return val;
        }

        public static string saveConfigValue(string key, string val)
        {
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                SQLiteParameter p1 = new SQLiteParameter("sysKey", key);
                SQLiteParameter p2 = new SQLiteParameter("sysVal", val);
                cmd.CommandText = @"UPDATE SYS_CFG SET SYS_VAL=@sysVal WHERE SYS_KEY=@sysKey";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.ExecuteNonQuery();
            }
            return val;
        }

        public static void loadAppConfig()
        {
            AppConfig.sessionTimeout = Int32.Parse(getConfigValue("SESSION_TIME"));
            AppConfig.breakTime = Int32.Parse(getConfigValue("BREAK_TIME"));
            AppConfig.processExcluded = getConfigValue("PROCESS_EXCLUDED");
            AppConfig.titleNotAllowed = getConfigValue("TITLE_PROHIBITED");
        }

        public static void saveAppConfig()
        {
            saveConfigValue("SESSION_TIME", AppConfig.sessionTimeout.ToString());
            saveConfigValue("BREAK_TIME", AppConfig.breakTime.ToString());
            saveConfigValue("PROCESS_EXCLUDED", AppConfig.processExcluded);
            saveConfigValue("TITLE_PROHIBITED", AppConfig.titleNotAllowed);
        }

        public static void restoreStat(UsageStat usageStat)
        {
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT CURR_DATE, TOTAL_TIME, SESS_TIME FROM RT_STATS WHERE CURR_DATE=@currDate";
                cmd.CommandType = CommandType.Text;
                SQLiteParameter p1 = new SQLiteParameter("currDate", DateTime.Today.Date);
                cmd.Parameters.Add(p1);
                SQLiteDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read();
                    int systemRunningTime = (int)r["TOTAL_TIME"];
                    int SessionTime = (int)r["SESS_TIME"];
                    usageStat.StartUpTime = DateTime.Now.AddSeconds(-systemRunningTime);
                    usageStat.SessionTime = SessionTime;
                }
                r.Close();
            }

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT LOG_DATE, PROCESS_NAME, WIN_TITLE, SPEND_TIME FROM HIST_STATS WHERE LOG_DATE=@currDate";
                cmd.CommandType = CommandType.Text;
                SQLiteParameter p1 = new SQLiteParameter("currDate", DateTime.Today.Date);
                cmd.Parameters.Add(p1);
                SQLiteDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    while (r.Read())
                    {
                        usageStat.addProgramTime((string)r["PROCESS_NAME"], (string)r["WIN_TITLE"], (int)r["SPEND_TIME"]);
                    }
                }
                r.Close();
            }
        }

        public static void saveProgramTime(UsageStat usageStat)
        {
            using (var tra = conn.BeginTransaction())
            {
                try
                {
                    foreach (KeyValuePair<string, int> entry in usageStat.programTime)
                    {
                        string program = entry.Key;
                        int spendTime = entry.Value;
                        string process = program.Substring(0, program.IndexOf(":"));
                        string title = program.Substring(program.IndexOf(":") + 1);
                        saveProgramTime(process, title, spendTime);
                    }
                    tra.Commit();
                }
                catch (Exception ex)
                {
                    tra.Rollback();
                    logger.Error("Failed to save to db", ex);
                }
            }
        }

        public static void saveProgramTime(string process, string title, int spendTime)
        {
            SQLiteParameter p1 = new SQLiteParameter("currDate", DateTime.Today.Date);
            SQLiteParameter p2 = new SQLiteParameter("processName", process);
            SQLiteParameter p3 = new SQLiteParameter("winTitle", title);
            SQLiteParameter p4 = new SQLiteParameter("spendTime", spendTime);

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                bool hasRows = false;
                cmd.CommandText = @"SELECT LOG_DATE, PROCESS_NAME, WIN_TITLE, SPEND_TIME FROM HIST_STATS 
                        WHERE LOG_DATE=@currDate AND PROCESS_NAME=@processName AND WIN_TITLE=@winTitle";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                SQLiteDataReader r = cmd.ExecuteReader();
                hasRows = r.HasRows;
                int spendTimeOld = 0;
                if (hasRows)
                {
                    r.Read();
                    spendTimeOld = (int)r["SPEND_TIME"];
                }
                r.Close();

                if (hasRows)
                {
                    if (spendTime != spendTimeOld)
                    {
                        using (SQLiteCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.CommandText = @"UPDATE HIST_STATS SET SPEND_TIME=@spendTime
                                    WHERE LOG_DATE=@currDate AND PROCESS_NAME=@processName AND WIN_TITLE=@winTitle";
                            updateCmd.CommandType = CommandType.Text;
                            updateCmd.Parameters.Add(p1);
                            updateCmd.Parameters.Add(p2);
                            updateCmd.Parameters.Add(p3);
                            updateCmd.Parameters.Add(p4);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    // today record not created
                    using (SQLiteCommand insertCmd = conn.CreateCommand())
                    {
                        insertCmd.CommandText = @"INSERT INTO HIST_STATS(LOG_DATE, PROCESS_NAME, WIN_TITLE, SPEND_TIME) 
                                VALUES(@currDate, @processName, @winTitle, @spendTime)";
                        insertCmd.CommandType = CommandType.Text;
                        insertCmd.Parameters.Add(p1);
                        insertCmd.Parameters.Add(p2);
                        insertCmd.Parameters.Add(p3);
                        insertCmd.Parameters.Add(p4);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void saveUsageTime(UsageStat usageStat)
        {
            SQLiteParameter p1 = new SQLiteParameter("currDate", DateTime.Today.Date);
            SQLiteParameter p2 = new SQLiteParameter("totalTime", Math.Ceiling(usageStat.SystemRunningTime.TotalSeconds));
            SQLiteParameter p3 = new SQLiteParameter("sessTime", usageStat.SessionTime);

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                bool hasRows = false;
                cmd.CommandText = @"SELECT CURR_DATE, TOTAL_TIME, SESS_TIME FROM RT_STATS WHERE CURR_DATE=@currDate";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(p1);
                SQLiteDataReader r = cmd.ExecuteReader();
                hasRows = r.HasRows;
                r.Close();
                if (hasRows)
                {
                    using (SQLiteCommand updateCmd = conn.CreateCommand())
                    {
                        updateCmd.CommandText = @"UPDATE RT_STATS SET TOTAL_TIME=@totalTime, SESS_TIME=@sessTime WHERE CURR_DATE=@currDate";
                        updateCmd.CommandType = CommandType.Text;
                        updateCmd.Parameters.Add(p1);
                        updateCmd.Parameters.Add(p2);
                        updateCmd.Parameters.Add(p3);
                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // today record not created
                    using (SQLiteCommand insertCmd = conn.CreateCommand())
                    {
                        insertCmd.CommandText = @"INSERT INTO RT_STATS(CURR_DATE, TOTAL_TIME, SESS_TIME) VALUES(@currDate, @totalTime, @sessTime)";
                        insertCmd.CommandType = CommandType.Text;
                        insertCmd.Parameters.Add(p1);
                        insertCmd.Parameters.Add(p2);
                        insertCmd.Parameters.Add(p3);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}

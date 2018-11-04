using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using log4net;

namespace KidsComputerGuard
{
    public partial class FormMain : Form
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FormMain));

        private BreakReminder breakReminder = new BreakReminder();
        private UsageStat usageStat = new UsageStat();
        private AppBlocker _appBlocker;
        private Boolean paused = false;

        public FormMain()
        {
            InitializeComponent();

            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(OnSystemEventsSessionSwitch);

            DbHelper.loadAppConfig();
            DbHelper.restoreStat(usageStat);

            _appBlocker = new AppBlocker(usageStat);

            // update ui
            updateForm();
        }

        public void OnApplicationExit(object sender, EventArgs e)
        {
            try
            {
                // Ignore any errors that might occur while closing the file handle.
                logger.Info("Kids guard shutdown - OnApplicationExit");
            }
            catch { }
        }

        public void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                // Ignore any errors that might occur while closing the file handle.
                logger.Info("Kids guard shutdown - OnProcessExit");
            }
            catch { }
        }

        /*
        // hide in task manager application tab
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;  // Turn on WS_EX_TOOLWINDOW
                return cp;
            }
        }
        */

        private void updateForm()
        {
            if (usageStat.State == "LOCK")
            {
                lblSessionRunningTime.ForeColor = Color.FromArgb(255, 0, 0);
                lblSessionRunningTime.Text = ((TimeSpan)(usageStat.LockStartTime.AddMinutes(AppConfig.breakTime) - DateTime.Now)).ToString("c").Substring(0, 8);
            }
            else
            {
                // update UI
                lblSessionRunningTime.ForeColor = Color.FromArgb(0, 64, 0);
                toolStripStatusLabel1.Text = "System Running Time: " + usageStat.SystemRunningTime.ToString("c").Substring(0, 8);
                lblSessionRunningTime.Text = TimeSpan.FromSeconds(usageStat.SessionTime).ToString("c").Substring(0, 8);
            }
        }

        private int updateStatInterval = AppConfig.updateStatInterval; // update usage statistics interval, default to 10 seconds
        private int reminderInterval = AppConfig.reminderInterval;  // station lock checking interval, 30 seconds
        private int saveInterval = AppConfig.saveInterval;  // save stat to db interval, 60 seconds
        private int sessionTime = AppConfig.sessionTimeout * 60; // remaining session time in seconds
        private int breakTime = AppConfig.breakTime * 60; // remaining break time in seconds
        private HashSet<String> programPersistSet = new HashSet<String>();

        bool isDaemonRunning()
        {
            Process[] processes = Process.GetProcessesByName("KidsGuardDaemon");
            return (processes != null && processes.Length > 0);
        }

        void runDaemon()
        {
            logger.Info("Start Daemon");
            Process myProcess = new Process();
            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = "KidsGuardDaemon.exe";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
                // This code assumes the process you are starting will terminate itself. 
                // Given that is is started without a window so you cannot terminate it 
                // on the desktop, it must terminate itself or you can do it programmatically
                // from this application using the Kill method.
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!isDaemonRunning())
            {
                runDaemon();
            }

            int timerInterval = timer1.Interval / 1000;

            // if station is locked, don't do anything
            if (stationLocked)
            {
                return;
            }

            if (usageStat.State == "LOCK")
            {
                if (paused)
                {
                    return;
                }

                if (((TimeSpan)(DateTime.Now - usageStat.LockStartTime)).TotalSeconds >= AppConfig.breakTime*60)
                {
                    logger.Info("Lock Period Ended");
                    usageStat.RestartSession();
                    saveStat();
                }
                else
                {
                    reminderInterval--;
                    if (reminderInterval == 0)
                    {
                        reminderInterval = AppConfig.reminderInterval;
                        // unlock during locking period, lock again
                        logger.Info("Within Lock Period, relock station!!!");
                        Win32.LockWorkStation();
                        return;
                    }
                }
                updateForm();
            }
            else
            {
                timer1.Enabled = false;

                // if paused, don't do anything
                if (!paused)
                {
                    // update running time
                    usageStat.UpdateSessionTime(1);
                    updateForm();
                }

                // update process using time
                String title = Win32.GetActiveWindowTitle();
                string process = Win32.GetActiveProcessName();

                updateStatInterval--;
                if (updateStatInterval == 0)
                {
                    updateStatInterval = AppConfig.updateStatInterval;
                    if (!AppConfig.isProcessExcluded(process))
                    {
                        usageStat.addProgramTime(process, title, 10);
                        programPersistSet.Add(process + ":" + title);

                    }
                }

                // save to db every one minute
                saveInterval--;
                if (saveInterval == 0)
                {
                    saveInterval = AppConfig.saveInterval;
                    saveStat();
                }

                // kill prohibited process
                if (!AppConfig.isTitleAllowed(title))
                {
                    Win32.KillActiveProcess();
                }

                _appBlocker.process(process, title, timerInterval);

                // break reminder
                if (!paused)
                {
                    breakReminder.checkBreakTime(usageStat);
                }

                timer1.Enabled = true;
            }
        }

        private void saveStat()
        {
            DbHelper.saveUsageTime(this.usageStat);

            foreach (String program in programPersistSet)
            {
                int spendTime = this.usageStat.programTime[program];
                string process = program.Substring(0, program.IndexOf(":"));
                string title = program.Substring(program.IndexOf(":") + 1);
                DbHelper.saveProgramTime(process, title, spendTime);
            }
            programPersistSet.Clear();
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            /*
             * don't need to minimize to notification area, so we would know whether it is running
             * 
            notifyIcon1.BalloonTipTitle = "Kids Guard";
            notifyIcon1.BalloonTipText = "Kids Guard";

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
            */
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*
            this.Show();
            this.WindowState = FormWindowState.Normal;
            */
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private bool checkPassword()
        {
            bool result = false;
            FormPassword frmPwd = new FormPassword(DbHelper.getConfigValue("PASSWORD"));
            if (frmPwd.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of testDialog's TextBox.
                result = true;
            }
            frmPwd.Dispose();

            return result;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (!paused)
            {
                // try to pause the timer
                if (checkPassword())
                {
                    paused = true;
                    buttonPause.Text = "Resume";
                    DbHelper.saveUsageTime(this.usageStat);
                }
            }
            else
            {
                paused = false;
                buttonPause.Text = "Pause";
            }
        }

        public DateTime lastLockTime = DateTime.Now;
        private bool stationLocked = false;
        void OnSystemEventsSessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                stationLocked = true;
                //I left my desk
                logger.Info("Session Locked");
                lastLockTime = DateTime.Now;
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                stationLocked = false;
                //I returned to my desk
                logger.Info("Session Unlocked");
                if (usageStat.State == "LOCK" && ((TimeSpan)(DateTime.Now - lastLockTime)).TotalMinutes > AppConfig.breakTime)
                {
                    logger.Info("Lock Period Ended");
                    usageStat.RestartSession();
                    saveStat();
                }
            }
        }

        private void currentStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormStat frmStat = new FormStat(usageStat);
            frmStat.Show();
        }

        private void toolStripButtonViewStat_Click(object sender, EventArgs e)
        {
            currentStatisticsToolStripMenuItem_Click(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.ShowDialog();
            aboutBox.Dispose();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FormChangePassword form = new FormChangePassword(DbHelper.getConfigValue("PASSWORD"));
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                DbHelper.saveConfigValue("PASSWORD", form.textBoxNewPassword.Text);
            }
            form.Dispose();
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!checkPassword())
                return;

            FormConfig form = new FormConfig();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                DbHelper.saveAppConfig();
            }
            form.Dispose();
        }

        private void buttonSetTimer_Click(object sender, EventArgs e)
        {
            if (!checkPassword())
                return;

            FormSetTimer frmSetTimer = new FormSetTimer();
            if (frmSetTimer.ShowDialog(this) == DialogResult.OK)
            {
                this.usageStat.RestartSession();
                this.usageStat.SessionTime = Int32.Parse(frmSetTimer.textBoxTimer.Text) * 60;
                this.updateForm();
                DbHelper.saveUsageTime(this.usageStat);
            }
            frmSetTimer.Dispose();
        }
    }
}

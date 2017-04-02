using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

using log4net;
using log4net.Config;

namespace KidsComputerGuard
{
    static class Program
    {
        private static string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";

        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(false, "Global\\" + Environment.UserName + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    return;
                }

                XmlConfigurator.Configure(new System.IO.FileInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/log4j.xml"));
                logger.Info("Kids guard started. Current User: " + Environment.UserName);

                // Handle the ApplicationExit event to know when the application is exiting.

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new FormMain());
            }

        }
    }
}

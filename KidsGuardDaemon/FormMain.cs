using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KidsGuardDaemon
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isKidsGuardRunning())
            {
                //MessageBox.Show("Kidsguard is running");
                return;
            }

            Process myProcess = new Process();
            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = "KidsGuard.exe";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
                // This code assumes the process you are starting will terminate itself. 
                // Given that is is started without a window so you cannot terminate it 
                // on the desktop, it must terminate itself or you can do it programmatically
                // from this application using the Kill method.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        bool isKidsGuardRunning()
        {
            Process[] processes = Process.GetProcessesByName("KidsGuard");
            return (processes != null && processes.Length > 0);
        }
    }
}

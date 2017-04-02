using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KidsComputerGuard
{
    public partial class FormConfig : Form
    {
        public FormConfig()
        {
            InitializeComponent();

            this.textBoxBreakTime.Text = AppConfig.breakTime.ToString();
            this.textBoxSessionTime.Text = AppConfig.sessionTimeout.ToString();
            this.textBoxProcessExcluded.Text = AppConfig.processExcluded;
            this.textBoxTitleProhibited.Text = AppConfig.titleNotAllowed;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            AppConfig.breakTime = Int32.Parse(this.textBoxBreakTime.Text);
            AppConfig.sessionTimeout = Int32.Parse(this.textBoxSessionTime.Text);
            AppConfig.processExcluded = this.textBoxProcessExcluded.Text;
            AppConfig.titleNotAllowed = this.textBoxTitleProhibited.Text;
        }
    }
}

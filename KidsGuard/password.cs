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
    public partial class FormPassword : Form
    {
        private string pwd;

        public FormPassword(string pwd)
        {
            InitializeComponent();
            this.pwd = pwd;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!txtPassword.Text.Equals(pwd))
            {
                System.Windows.Forms.MessageBox.Show(null, "Wrong Password!!!", "KidsGuard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
            }

        }
    }
}

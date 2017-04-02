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
    public partial class FormChangePassword : Form
    {
        string pwd;

        public FormChangePassword(string pwd)
        {
            InitializeComponent();

            this.pwd = pwd;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!this.textBoxOldPassword.Text.Equals(pwd))
            {
                System.Windows.Forms.MessageBox.Show(null, "Wrong Password!!!", "KidsGuard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!textBoxNewPassword.Text.Equals(textBoxNewPassword1.Text))
            {
                System.Windows.Forms.MessageBox.Show(null, "Password not consistent!!!", "KidsGuard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
        }
    }
}

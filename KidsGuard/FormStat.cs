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
    public partial class FormStat : Form
    {
        private UsageStat usageStat;

        public FormStat(UsageStat p_usageStat)
        {
            InitializeComponent();
            usageStat = p_usageStat;

            updateForm();
        }

        private void updateForm()
        {
            // update UI
            dgv1.Enabled = false;
            foreach (KeyValuePair<string, int> entry in usageStat.programTime)
            {
                string program = entry.Key;
                string processName = program.Substring(0, program.IndexOf(":"));
                string title = program.Substring(program.IndexOf(":") + 1);
                updateDataGridView(processName, title, entry.Value);
            }
            dgv1.Sort(dgv1.Columns[2], ListSortDirection.Descending);
            dgv1.Enabled = true;
        }

        private void updateDataGridView(string processName, string title, int spendTime)
        {
            TimeSpan ts = TimeSpan.FromSeconds(spendTime);
            foreach (DataGridViewRow row in dgv1.Rows)
            {
                if (row.Cells[0].Value.ToString().Equals(processName) && row.Cells[1].Value.ToString().Equals(title))
                {
                    row.Cells[2].Value = ts.ToString("c").Substring(0, 8);
                    return;
                }
            }

            dgv1.Rows.Add(processName, title, ts.ToString("c").Substring(0, 8));
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            updateForm();
        }
    }
}

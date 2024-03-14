using DocumentFormat.OpenXml.Office2010.CustomUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace ES_GUI
{
    public partial class FrmAdjust : Form
    {
        private DataGridViewSelectedCellCollection dataGridViewSelectedCellCollection;

        public FrmAdjust(ref DataGridViewSelectedCellCollection sel)
        {
            InitializeComponent();
            dataGridViewSelectedCellCollection = sel;

        }

        private void rbAdd_Click(object sender, EventArgs e)
        {
            tbAdd.Focus();
        }

        private void rbPercent_Click(object sender, EventArgs e)
        {
            tbPercent.Focus();
        }

        private void rbSet_Click(object sender, EventArgs e)
        {
            tbSet.Focus();
        }

        private void tbPercent_Enter(object sender, EventArgs e)
        {
            rbPercent.Checked = true;
        }

        private void tbPercent_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\u001b')
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = "0";
            }
        }

        private void textBoxValidating(object sender, CancelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text != string.Empty)
            {
                if (!CheckPercentage(textBox.Text.ToString()))
                {
                    errorProvider1.SetError(textBox, "Invalid input");
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(textBox, "");
                }
            }
        }

        private void tbAdd_Enter(object sender, EventArgs e)
        {
            rbAdd.Checked = true;
        }

        private void tbSet_Enter(object sender, EventArgs e)
        {
            rbSet.Checked = true;
        }

        public bool CheckPercentage(string string_8)
        {
            double result;
            return double.TryParse(Convert.ToString(string_8), NumberStyles.Any, NumberFormatInfo.CurrentInfo, out result);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rbPercent.Checked)
            {
                if (tbPercent.Text == string.Empty || tbPercent.Text == "0")
                {
                    return;
                }
                for (int i = 0; i < dataGridViewSelectedCellCollection.Count; i++)
                {
                    float value = float.Parse(dataGridViewSelectedCellCollection[i].Value.ToString());
                    dataGridViewSelectedCellCollection[i].Value = (value + value / 100f * float.Parse(tbPercent.Text));
                }
            }
            else if (rbAdd.Checked)
            {
                if (tbAdd.Text == string.Empty)
                {
                    return;
                }
                for (int j = 0; j < dataGridViewSelectedCellCollection.Count; j++)
                {
                    float value = float.Parse(dataGridViewSelectedCellCollection[j].Value.ToString());
                    dataGridViewSelectedCellCollection[j].Value = value + float.Parse(tbAdd.Text);
                }
            }
            else if (rbSet.Checked)
            {
                if (tbSet.Text == string.Empty)
                {
                    return;
                }

                for (int k = 0; k < dataGridViewSelectedCellCollection.Count; k++)
                {
                    dataGridViewSelectedCellCollection[k].Value = float.Parse(tbSet.Text);
                }
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            dataGridViewSelectedCellCollection = null;
            tbAdd.Text = "0";
            tbPercent.Text = "0";
            tbSet.Text = "0";
        }

        private void FrmAdjust_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
        }
    }
}

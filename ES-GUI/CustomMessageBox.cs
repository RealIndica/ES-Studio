using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ES_GUI
{
    public partial class CustomMessageBox : Form
    {
        public CustomMessageBox()
        {
            InitializeComponent();
            ThemeManager.ApplyTheme(this);
        }

        private void CustomMessageBox_Load(object sender, EventArgs e)
        {

        }

        public void SetMessage(string message, string title) 
        {
            titleLabel.Text = title;
            textLabel.Text = message;
        }

        private void dismissButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

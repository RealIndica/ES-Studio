using System;
using System.IO;
using System.Windows.Forms;
using ColorMapEditor.Classes;

namespace ColorMapEditor.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }
        
        private void btnLoadColormap_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse for Map File",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "map",
                Filter = "Colormap files (*.map)|*.map",
                FilterIndex = 2,
                RestoreDirectory = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbEditor.Text = File.ReadAllText(openFileDialog1.FileName);
            }

        }

        private void btnSaveColorMap_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Save Colormap Files";
            saveFileDialog1.CheckFileExists = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "map";
            saveFileDialog1.Filter = "Colormap Files (*.map)|*.map|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllLines(saveFileDialog1.FileName, tbEditor.Lines);
            }
        }

        private void spinboxRows_ValueChanged(object sender, EventArgs e)
        {
            table3DView1.yCount = (int)((NumericUpDown)sender).Value;
            table3DView1.SetTableSize();

        }

        private void spinboxColumns_ValueChanged(object sender, EventArgs e)
        {
            table3DView1.xCount = (int)((NumericUpDown)sender).Value;
            table3DView1.SetTableSize();
        }

        private void tbEditor_TextChanged(object sender, EventArgs e)
        {
            try
            {

                ColorHeatMap map = new ColorHeatMap(tbEditor.Lines);
                table3DView1.heatMap = map;
                table3DView1.LoadCellHeatMap();
            }
            catch { }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ES_GUI
{
    public partial class ColorSettings : Form
    {
        public ColorHeatMap heatmap { get; set; }
        public String HeatmapPath { get; set; }
        
        public ColorSettings()
        {
            InitializeComponent();
        }

        private void lbColormaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            heatmap = new ColorHeatMap($"{Directory.GetCurrentDirectory()}/assets/colormaps/{lbColormaps.Items[lbColormaps.SelectedIndex]}");
            HeatmapPath = lbColormaps.Items[lbColormaps.SelectedIndex].ToString();
            panel1.Refresh();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (heatmap != null)
            {
                if (heatmap.ColorsOfMap.Count > 0)
                {
                    LinearGradientBrush br = new LinearGradientBrush(this.ClientRectangle, Color.Black, Color.Black, 0, false);
                    ColorBlend cb = new ColorBlend();
                    cb.Colors = heatmap.ColorsOfMap.ToArray();
                    List<float> positions = new List<float>();
                    positions.Add(0f);

                    for (float i = 2; i < heatmap.ColorsOfMap.Count; i++)
                    {
                        positions.Add(i / heatmap.ColorsOfMap.Count);
                    }
                    positions.Add(1.0f);

                    cb.Positions = positions.ToArray();

                    br.InterpolationColors = cb;
                    // rotate
                    br.RotateTransform(30);
                    // paint
                    e.Graphics.FillRectangle(br, this.ClientRectangle);
                }
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ColorSettings_Load(object sender, EventArgs e)
        {
            DirectoryInfo d = new DirectoryInfo($"{Directory.GetCurrentDirectory()}/assets/colormaps/");

            FileInfo[] Files = d.GetFiles("*.map"); //Getting Text files
            lbColormaps.Items.Clear();
            foreach (FileInfo file in Files)
            {
                lbColormaps.Items.Add(file.Name);
            }
        }
    }
}
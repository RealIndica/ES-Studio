using ES_GUI.Properties;
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
        public Color fontColor { get; set; }
        public bool useAutoFont { get; set; }

        public ColorSettings()
        {
            InitializeComponent();
        }

        private void lbColormaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            heatmap = new ColorHeatMap($"{Directory.GetCurrentDirectory()}/assets/colormaps/{lbColormaps.Items[lbColormaps.SelectedIndex]}");
            HeatmapPath = lbColormaps.Items[lbColormaps.SelectedIndex].ToString();
            panel1.Refresh();
            updatePreviewTable();
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

        private void fontColourPreview_Paint(object sender, PaintEventArgs e)
        {
            if (fontColor != null)
            {
                SolidBrush brush = new SolidBrush(fontColor);
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
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

            lbColormaps.SelectedIndex = lbColormaps.Items.IndexOf(Settings.Default.HeatmapPath);
            fontAuto.Checked = useAutoFont = Settings.Default.HeatFontAuto;
            fontColor = Settings.Default.HeatFontColour;

            updatePreviewTable();

            ThemeManager.ApplyTheme(this);
        }

        private void fontAuto_CheckedChanged(object sender, EventArgs e)
        {
            useAutoFont = fontAuto.Checked;
            updatePreviewTable();
        }

        private void fontColPick_Click(object sender, EventArgs e)
        {
            ColorDialog clrDialog = new ColorDialog(); //until i make my own colour dialog form

            if (clrDialog.ShowDialog(this) == DialogResult.OK)
            {
                fontColor = clrDialog.Color;
                fontColourPreview.Invalidate();
                updatePreviewTable();
            }
        }

        private void updatePreviewTable()
        {
            int len = 6;

            while (previewTable.Columns.Count < len)
            {
                int columnIndex = previewTable.Columns.Count + 1;
                previewTable.Columns.Add($"Column{columnIndex}", $"Column {columnIndex}");
                previewTable.Columns[columnIndex - 1].Resizable = DataGridViewTriState.False;
            }

            previewTable.Rows.Clear();

            object[] firstRow = new object[len];
            for (int i = 0; i < len; i++)
            {
                firstRow[i] = (i + 1).ToString() + ".00";
            }
            previewTable.Rows.Add(firstRow);

            object[] secondRow = new object[len];
            for (int i = 0; i < len; i++)
            {
                secondRow[i] = (i + len).ToString() + ".00";
            }
            previewTable.Rows.Add(secondRow);

            foreach (DataGridViewRow row in previewTable.Rows)
            {
                row.Resizable = DataGridViewTriState.False;
                for (int i = 0; i < previewTable.Columns.Count; i++)
                {
                    DataGridViewCell cell = row.Cells[i];
                    double value = Convert.ToDouble(cell.Value);
                    cell.Style.BackColor = heatmap.GetColorForValue(value, len * 2, 1);

                    if (!useAutoFont)
                    {
                        cell.Style.ForeColor = fontColor;
                    } else
                    {
                        cell.Style.ForeColor = Helpers.GetReadableTextColor(cell.Style.BackColor);
                    }
                }
            }
        }
    }
}
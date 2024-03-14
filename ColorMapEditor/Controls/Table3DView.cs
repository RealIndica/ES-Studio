using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ColorMapEditor.Controls
{
    public partial class Table3DView : UserControl
    {        

        public static string Colormap = "";
        public static string ColormapDir = "";
        public static bool ColorizeHeaders = false;
        public ColorHeatMap heatMap;
        
        public int xCount = 10;
        public int yCount = 20;

        public DataGridView Grid => this.dataGridView1;
        
        public SplitContainer myPanel { get; set; }

        
        

        public Table3DView()
        {
            InitializeComponent();
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised;
            var newStyle = new DataGridViewAdvancedBorderStyle
            {
                Top = DataGridViewAdvancedCellBorderStyle.None,
                Left = DataGridViewAdvancedCellBorderStyle.Outset,
                Bottom = DataGridViewAdvancedCellBorderStyle.OutsetDouble,
                Right = DataGridViewAdvancedCellBorderStyle.OutsetDouble
            };

            DataGridViewAdvancedBorderStyle newStyle2 = new DataGridViewAdvancedBorderStyle();
            newStyle.Top = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
            newStyle.Left = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
            newStyle.Bottom = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
            newStyle.Right = DataGridViewAdvancedCellBorderStyle.OutsetDouble;

            dataGridView1.AdjustColumnHeaderBorderStyle(newStyle2, newStyle2, true, true);

            if (Colormap == string.Empty || ColormapDir == string.Empty)
            {
                heatMap = new ColorHeatMap();
            }

        }
        
        private void Table3DView_Load(object sender, EventArgs e)
        {
            dataGridView1.EnableHeadersVisualStyles = false;

            if (Colormap == string.Empty || ColormapDir == string.Empty)
            {
                heatMap = new ColorHeatMap();
            }
            else
            {
                heatMap = new ColorHeatMap(ColormapDir + Colormap);
            }
            SetTableSize();
            LoadCellHeatMap();
            dataGridView1.Font = new Font("Lucida Sans Unicode", 10, FontStyle.Bold);
            dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            dataGridView1.ColumnHeadersHeight = 30;
            foreach (DataGridViewRow row in dataGridView1.Rows)
                row.Height = 20;

            if (ColorizeHeaders == true)
            {
                LoadHeaderHeatMap();
                LoadRowHeaderHeatMap();
            }
            else
            {
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    DataGridViewColumnHeaderCell headerCell = column.HeaderCell;


                    headerCell.Style.BackColor = dataGridView1.DefaultCellStyle.BackColor;

                }
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var headerCell = row.HeaderCell;
                    headerCell.Style.BackColor = dataGridView1.DefaultCellStyle.BackColor;

                }
            }
        }

   



        public static T[,] Make2DArray<T>(T[] input, int height, int width)
        {
            var output = new T[height, width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }


        private void DataGridView1_CellFormatting_1(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 &&
                this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected)
            {
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);

                e.CellStyle.SelectionForeColor = Color.FromArgb(e.CellStyle.BackColor.ToArgb());
                e.CellStyle.SelectionBackColor = Color.FromArgb(e.CellStyle.BackColor.ToArgb() ^0xffffff);

            }
            else
            {
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Regular);
                e.CellStyle.SelectionForeColor = Color.Black;
            }
        }

        private void LoadHeaderHeatMap()
        {
            List<double> values = (from DataGridViewColumn column in dataGridView1.Columns select Convert.ToDouble(column.HeaderCell.Value)).ToList();
            if (!values.Any())
            {
                return;
            }

            var max = (values.Max() * 1.05);
            var min = values.Min();


            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                var headerCell = column.HeaderCell;

                var value = Convert.ToDouble(headerCell.Value);
                headerCell.Style.BackColor = heatMap.GetColorForValue(value, max, min);

            }
        }
        private void LoadRowHeaderHeatMap()
        {
            var values = (from DataGridViewRow row in dataGridView1.Rows select Convert.ToDouble(row.HeaderCell.Value)).ToList();
            if (values.Count() == 0)
            {
                return;
            }

            var max = (values.Max() * 1.05);
            var min = values.Min();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var headerCell = row.HeaderCell;
                var value = Convert.ToDouble(headerCell.Value);
                headerCell.Style.BackColor = heatMap.GetColorForValue(value, max, min);

            }
        }
        public void LoadCellHeatMap()
        {
            var values = new List<double>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                for (var i = 0; i < dataGridView1.Columns.Count; i++)
                    values.Add(Convert.ToDouble(row.Cells[i].Value));
            }
            if (!values.Any())
            {
                return;
            }

            var max = (values.Max() * 1.01);
            var min = values.Min();
            var colorRange = max - min;
            var colorOffset = min;
            

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    DataGridViewCell cell = row.Cells[i];
                    double value = Convert.ToDouble(cell.Value);
                    if (max != min)
                        cell.Style.BackColor = heatMap.GetColorForValue(value, max, min);
                    else
                        cell.Style.BackColor = Color.FromArgb(255, (byte)0, (byte)255, (byte)0);
                }

            }
        }
    

 
        public void SetTableSize()
        {
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns.Clear();

            for (int i = 0; i < xCount; i++)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = @"Col" + i;
                col.Name = "column" + i;
                dataGridView1.Columns.Add(col);
            }


            var xAx = new double[xCount];
            var yAx = new double[yCount];
            var table = new double[xCount * yCount];
            for (var i = 1; i < xCount+1; i++)
            {
                xAx[i-1] = i*10;
            }
            for (var i = 0; i < yCount; i++)
            {
                yAx[i] = i * 500;
            }
            for(var i = 0; i < xCount * yCount; i++)
                table[i] = i*100;






            var twoDArray = Make2DArray(table, xCount, yCount);
            for (int i = 0; i < yCount; i++)
            {
                var row = (DataGridViewRow)dataGridView1.RowTemplate.Clone();
                row.CreateCells(dataGridView1);
                for (int k = 0; k < xCount; k++)
                {
                    try
                    {
                        row.Cells[k].Value = twoDArray[k, i];
                    }
                    catch (Exception ex)
                    {
                    }
                }
                dataGridView1.Rows.Add(row);
            }


            for (int i = 0; i < yCount; i++)
                dataGridView1.Rows[i].HeaderCell.Value = yAx[i].ToString(CultureInfo.InvariantCulture);

            for (int i = 0; i < xCount; i++)
                dataGridView1.Columns[i].HeaderCell.Value = xAx[i].ToString(CultureInfo.InvariantCulture);

            foreach (DataGridViewColumn item in dataGridView1.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;
                item.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                item.Width = 50;
            }
            foreach (DataGridViewRow item in dataGridView1.Rows)
            {
                item.Height = 20;
            }
            

            LoadCellHeatMap();

        }

    }
}
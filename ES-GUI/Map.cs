using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using AquaControls;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
using ES_GUI.Properties;
using System.IO;

namespace ES_GUI
{
    public enum MapParam
    {
        None,
        RPM,
        Throttle,
        ManifoldPressure,
        Gear,
        SpeedMPH,
        SpeedKMH,
        Clutch,
        Load
    }

    public enum MapControlParam
    {
        None,
        RevLimit,
        IgnitionAdvance,
        ActiveCylinders,
        ActiveCylindersRandom,
        ActiveCylindersRandomUpdateTime,
        TargetAFR
    }

    public enum MapParamType
    {
        X,
        Y
    }

    [Serializable]
    public struct SerializableMapData
    {
        public DataTable dataTable;
        public MapParam xParam;
        public MapParam yParam;
        public MapControlParam controlParam;
        public string name;
    }

    public class Map
    {
        public MapParam xParam = 0;
        public MapParam yParam = 0;
        public MapControlParam controlParam = 0;

        public string name;

        ESClient client;

        public bool enabled = false;
        private bool tableReadyEdit = false;

        private float minCellValue = 0f;
        private float maxCellValue = 0f;

        private object originalCellValue;

        private DataGridView gridView;
        public DataTable dataTable;

        private CustomTabControl parentTabControl;
        private TabPage parentPage;
        private Label xName;
        private Label yName;
        private Label ctrlName;
        private Label ignitionModuleLabel;
        private Label mapEnabledLabel;
        private PictureBox ignitionModuleIcon;
        private PictureBox mapEnabledIcon;
        private Button generateMapButton;
        private Button enableButton;
        private Button disableButton;
        private Button deleteMapButton;
        private Button btnAdjustTable;
        private Form mainForm;

        public MapController mapController;

        public PictureBox tableOverlay;
        
        public ColorHeatMap HeatMap;

        
        public DataGridView GridView
        {
            get { return gridView; }
        }

        public Map(Form parentForm) 
        {
            mainForm = parentForm;
        }

        public SerializableMapData ToSerializableData()
        {
            return new SerializableMapData
            {
                dataTable = this.dataTable,
                xParam = this.xParam,
                yParam = this.yParam,
                controlParam = this.controlParam,
                name = this.name
            };
        }

        public void LoadFromSerializableData(SerializableMapData data, CustomTabControl parentTab, ESClient inclient)
        {
            Configure(data.xParam, data.yParam, data.controlParam, data.name);
            Create(parentTab, inclient);
            dataTable = data.dataTable;
            gridView.DataSource = dataTable;

            getParamDataList(dataTable.Rows.Count, xParam, MapParamType.X);
            List<string> yParamData = getParamDataList(dataTable.Rows.Count, yParam, MapParamType.Y);

            for (int i = 0; i <= dataTable.Rows.Count - 1; i++)
            {
                gridView.Rows[i].HeaderCell.Value = yParamData[i];
            }

            updateMapCellData();
            CheckCellHiLo();
            updateHeatMap();
            BuildData();
            SetTableProperties();
            SetHeatMapColor(new ColorHeatMap($"{Directory.GetCurrentDirectory()}/assets/colormaps/{Settings.Default.HeatmapPath}"));
            tableReadyEdit = true;
        }

        private bool updateMapCellData()
        {
            if (mapController.cellRectangle.X == 0 || mapController.cellRectangle.Y == 0 || mapController.cellRectangle.Width == 0 || mapController.cellRectangle.Height == 0
                || mapController.cellOffset.X == 0 || mapController.cellOffset.Y == 0 || mapController.tablePoint.Width == 0 || mapController.tablePoint.Height == 0)
            {
                mapController.cellRectangle = gridView.GetCellDisplayRectangle(gridView.ColumnCount - 1, gridView.RowCount - 1, false);
                mapController.cellOffset.X = gridView.Rows[0].HeaderCell.Size.Width;
                mapController.cellOffset.Y = gridView.Columns[0].HeaderCell.Size.Height;
                mapController.tablePoint.Width = mapController.cellRectangle.Width;
                mapController.tablePoint.Height = mapController.cellRectangle.Height;
                Debug.WriteLog("Updated Map Cell Data");
                BuildData();
                return true;
            }
            return false;
        }

        private void manageGlobalControlParams(bool active)
        {
            switch (controlParam)
            {
                case MapControlParam.RevLimit:
                    client.edit.useRpmTable = active;
                    break;
                case MapControlParam.IgnitionAdvance:
                    client.edit.useIgnTable = active;
                    break;
                case MapControlParam.ActiveCylinders:
                    client.edit.useCylinderTable = active;
                    break;
                case MapControlParam.ActiveCylindersRandom:
                    client.edit.useCylinderTableRandom = active;
                    break;
                case MapControlParam.ActiveCylindersRandomUpdateTime:
                    break;
                case MapControlParam.TargetAFR:
                    client.edit.useAfrTable = active;
                    break;
            }
        }

        public void enable(bool focus = false)
        {
            if (focus)
            {
                parentTabControl.SelectedTab = parentPage;
                while (updateMapCellData()) { Thread.Sleep(100); }
            }
            manageGlobalControlParams(true);
            mapEnabledIcon.Image = Resources.check;
            enabled = true;
            tableOverlay.Parent = gridView;
            tableOverlay.Size = gridView.Size;;
        }

        public void disable(bool focus = false) 
        {
            if (focus)
            {
                parentTabControl.SelectedTab = parentPage;
                while (updateMapCellData()) { Thread.Sleep(100); }
            }
            manageGlobalControlParams(false);
            mapEnabledIcon.Image = Resources.cross;
            enabled = false;
            tableOverlay.Parent = null;
            tableOverlay.Size = new Size(0, 0);
        }

        public void Update()
        {
            if (client.edit.useCustomIgnitionModule)
            {
                ignitionModuleIcon.Image = Resources.check;
            } else
            {
                ignitionModuleIcon.Image = Resources.cross;
            }

            if (!enabled) return;
            if (gridView.ColumnCount <= 0 || gridView.RowCount <= 0) return;

            updateMapCellData();

            mapController.xValue = GetMapValue(xParam);
            mapController.yValue = GetMapValue(yParam);
            mapController.UpdateTablePos();

            switch (controlParam)
            {
                case MapControlParam.RevLimit:
                    client.edit.customRevLimit = mapController.Pos2Val(true);
                    break;
                case MapControlParam.IgnitionAdvance:
                    client.edit.customSpark = mapController.Pos2Val();
                    break;
                case MapControlParam.ActiveCylinders:
                    client.edit.activeCylinderCount = (int)mapController.Pos2Val(true);
                    break;
                case MapControlParam.ActiveCylindersRandom:
                    client.edit.activeCylinderCount = (int)mapController.Pos2Val(true);
                    break;
                case MapControlParam.ActiveCylindersRandomUpdateTime:
                    client.edit.activeCylindersRandomUpdateTime = (int)mapController.Pos2Val(true);
                    break;
                case MapControlParam.TargetAFR:
                    client.edit.targetAfr = mapController.Pos2Val(false);
                    break;
            }
        }

        private double GetMapValue(MapParam param)
        {
            switch (param)
            {
                case MapParam.RPM:
                    return client.update.RPM;
                case MapParam.Throttle:
                    return client.update.tps;
                case MapParam.ManifoldPressure:
                    return client.update.manifoldPressure;
                case MapParam.Gear:
                    return client.update.gear + 1;
                case MapParam.SpeedMPH:
                    return client.update.vehicleSpeed /= (1000.0 / (60 * 60));
                case MapParam.SpeedKMH:
                    return client.update.vehicleSpeed /= (1609.344 / (60 * 60));
                case MapParam.Clutch:
                    return client.update.clutchPosition;
                case MapParam.Load:
                    return client.update.engineLoad;
                default:
                    return 0;
            }
        }
        
        public void SetHeatMapColor(ColorHeatMap heatMap)
        {
            HeatMap = null;
            HeatMap = heatMap;
            updateHeatMap();
        }

        private void setControllerValue(MapParamType t, double val, bool setMax)
        {
            switch (t)
            {
                case MapParamType.X:
                    if (setMax)
                        mapController.maxX = val;
                    else
                        mapController.xValue = val;
                    break;
                case MapParamType.Y:
                    if (setMax) 
                        mapController.maxY = val;
                    else 
                        mapController.yValue = val;
                    break;
            }
        }

        private List<string> getParamDataList(int size, MapParam param, MapParamType t)
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < size; i++)
            {
                switch (param)
                {
                    case MapParam.None: return new List<string>() { "0" };
                    case MapParam.RPM:
                        ret.Add(Math.Round((client.update.maxRPM / (size - 1)) * i).ToString());
                        setControllerValue(t, client.update.maxRPM, true);
                        break;
                    case MapParam.Throttle:
                        ret.Add((5 * i).ToString());
                        setControllerValue(t, 1, true);
                        break;
                    case MapParam.ManifoldPressure:
                        ret.Add(Math.Round((102000.0 / (size - 1)) * i).ToString());
                        setControllerValue(t, 102000, true);
                        break;
                    case MapParam.Gear:
                        ret.Add(i.ToString());
                        setControllerValue(t, 19, true);
                        break;
                    case MapParam.SpeedMPH:
                        ret.Add(Math.Round((200.0 / (size - 1)) * i).ToString());
                        setControllerValue(t, 200, true);
                        break;
                    case MapParam.SpeedKMH:
                        ret.Add(Math.Round((300.0 / (size - 1)) * i).ToString());
                        setControllerValue(t, 300, true);
                        break;
                    case MapParam.Clutch:
                        ret.Add((5 * i).ToString());
                        setControllerValue(t, 1, true);
                        break;
                    case MapParam.Load:
                        ret.Add((5 * i).ToString());
                        setControllerValue(t, 100, true);
                        break;
                    default: return new List<string>();
                }
            }
            return ret;
        }

        #region Creation
        public void Configure(MapParam xAxis, MapParam yAxis, MapControlParam outControl, string mapName)
        {
            xParam = xAxis;
            yParam = yAxis;
            controlParam = outControl;
            name = mapName;
        }

        public void Delete()
        {
            disable();
            client.customMaps.Remove(this);
            parentTabControl.TabPages.Remove(parentPage);
            parentTabControl.AdjustTabSizes();
        }

        public void Create(CustomTabControl parentTab, ESClient inclient)
        {
            TabPage newTab = new TabPage();
            parentPage = newTab;
            newTab.Text = name;

            parentTabControl = parentTab;
            client = inclient;

            dataTable = new DataTable();
            mapController = new MapController();

            gridView = new DataGridView();
            gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders;
            gridView.RowHeadersWidth = 75;
            gridView.AllowUserToAddRows = false;
            gridView.Location = new Point(19, 19);
            gridView.Size = new Size(1101, 380);
            newTab.Controls.Add(gridView);

            xName = new Label();
            xName.Text = xParam.ToString();
            xName.Location = new Point(16, 3);
            xName.Size = new Size(200, 13);
            newTab.Controls.Add(xName);

            yName = new Label();
            yName.Text = string.Join("\n", yParam.ToString().ToCharArray());
            yName.Location = new Point(1, 40);
            yName.TextAlign = ContentAlignment.MiddleCenter;
            yName.Size = new Size(20, 250);
            newTab.Controls.Add(yName);

            generateMapButton = new Button();
            generateMapButton.Text = "Regenerate Map";
            generateMapButton.Location = new Point(1018, 405);
            generateMapButton.Size = new Size(102, 23);
            generateMapButton.Click += regenMapButton_Click;
            newTab.Controls.Add(generateMapButton);

            enableButton = new Button();
            enableButton.Text = "Enable";
            enableButton.Location = new Point(1018, 500);
            enableButton.Size = new Size(102, 23);
            enableButton.Click += enableButton_Click;
            newTab.Controls.Add(enableButton);

            disableButton = new Button();
            disableButton.Text = "Disable";
            disableButton.Location = new Point(1018, 529);
            disableButton.Size = new Size(102, 23);
            disableButton.Click += disableButton_Click;
            newTab.Controls.Add(disableButton);

            deleteMapButton = new Button();
            deleteMapButton.Text = "Delete Map";
            deleteMapButton.Location = new Point(1018, 434);
            deleteMapButton.Size = new Size(102, 23);
            deleteMapButton.ForeColor = Color.Red;
            deleteMapButton.Click += deleteMapButton_Click;
            newTab.Controls.Add(deleteMapButton);

            btnAdjustTable = new Button();
            btnAdjustTable.Location = new Point(19, 434);
            btnAdjustTable.Size = new Size(102, 23);
            btnAdjustTable.Text = "Adjust Selection";
            btnAdjustTable.Click += btnAdjustTable_Click;
            newTab.Controls.Add(btnAdjustTable);

            ctrlName = new Label();
            ctrlName.Text = controlParam.ToString();
            ctrlName.Location = new Point(16, 402);
            ctrlName.Size = new Size(200, 13);
            newTab.Controls.Add(ctrlName);

            ignitionModuleLabel = new Label();
            ignitionModuleLabel.Text = "Ignition Module";
            ignitionModuleLabel.Font = new Font(ignitionModuleLabel.Font.FontFamily, 14.25f, FontStyle.Regular);
            ignitionModuleLabel.Location = new Point(15, 499);
            ignitionModuleLabel.Size = new Size(139, 24);
            newTab.Controls.Add(ignitionModuleLabel);

            mapEnabledLabel = new Label();
            mapEnabledLabel.Text = "Map Enabled";
            mapEnabledLabel.Font = new Font(mapEnabledLabel.Font.FontFamily, 14.25f, FontStyle.Regular);
            mapEnabledLabel.Location = new Point(15, 528);
            mapEnabledLabel.Size = new Size(139, 24);
            newTab.Controls.Add(mapEnabledLabel);

            ignitionModuleIcon = new PictureBox();
            ignitionModuleIcon.SizeMode = PictureBoxSizeMode.Zoom;
            ignitionModuleIcon.Image = Resources.cross;
            ignitionModuleIcon.Size = new Size(25, 25);
            ignitionModuleIcon.Location = new Point(160, 499);
            newTab.Controls.Add(ignitionModuleIcon);

            mapEnabledIcon = new PictureBox();
            mapEnabledIcon.SizeMode = PictureBoxSizeMode.Zoom;
            mapEnabledIcon.Image = Resources.cross;
            mapEnabledIcon.Size = new Size(25, 25);
            mapEnabledIcon.Location = new Point(160, 528);
            newTab.Controls.Add(mapEnabledIcon);

            tableOverlay = new PictureBox();
            tableOverlay.Parent = null;
            tableOverlay.Size = new Size(0, 0);
            tableOverlay.Location = new Point(0, 0);
            tableOverlay.BackColor = Color.Transparent;
            tableOverlay.Paint += new PaintEventHandler(tableOverlayPaint);

            gridView.CellValueChanged += gridView_CellValueChanged;
            gridView.KeyDown += gridView_KeyDown;
            gridView.CellBeginEdit += gridView_CellBeginEdit;

            parentTabControl.TabPages.Insert(parentTabControl.TabPages.Count - 1, newTab);
            client.customMaps.Add(this);
            ThemeManager.ApplyTheme(parentPage);
            if (HeatMap == null)
            {
                HeatMap = new ColorHeatMap();
            }

        }

        public void BuildTable(bool autoGradient = false)
        {
            if (client.isConnected)
            {
                tableReadyEdit = false;
                dataTable.Clear();
                dataTable = new DataTable();
                gridView.DataSource = dataTable;

                List<double> entries = new List<double>();

                switch (controlParam) //output control
                {
                    case MapControlParam.None: return;
                    case MapControlParam.RevLimit:
                        entries = Enumerable.Repeat(client.update.maxRPM, 20).ToList();
                        break;
                    case MapControlParam.IgnitionAdvance: 
                        entries = client.update.sparkTimingList;
                        autoGradient = true;
                        break;
                    case MapControlParam.ActiveCylinders:
                        entries = Enumerable.Repeat((double)client.update.cylinderCount, 21).ToList();
                        break;
                    case MapControlParam.ActiveCylindersRandom:
                        entries = Enumerable.Repeat((double)client.update.cylinderCount, 21).ToList();
                        break;
                    case MapControlParam.ActiveCylindersRandomUpdateTime:
                        entries = Enumerable.Repeat(1d, 21).ToList();
                        break;
                    case MapControlParam.TargetAFR:
                        entries = Enumerable.Repeat(13.5d, 21).ToList();
                        break;
                    default: return;
                }

                foreach (string t in getParamDataList(entries.Count, xParam, MapParamType.X))
                {
                    dataTable.Columns.Add(t);
                }

                List<double> last = new List<double>();
                last.AddRange(entries);

                double offset = 1;
                double min = last.OrderBy(x => x).ElementAt(1);

                List<List<double>> processed = new List<List<double>>();

                if (yParam != MapParam.None)
                {
                    for (int i = 0; i <= 20; i++)
                    {
                        List<double> t = new List<double>();

                        if (i == 0)
                        {
                            processed.Add(entries);
                            continue;
                        }

                        for (int s = 0; s < last.Count; s++)
                        {
                            if (s != 0 && autoGradient)
                                t.Add((last[s] - (offset)).Clamp(min, 100000f));
                            else
                                t.Add(last[s]);
                        }

                        processed.Add(t);
                        last.Clear();
                        last.AddRange(t);
                    }

                    processed.Reverse();
                } else
                {
                    processed.Add(entries);
                }

                List<string> yParamData = getParamDataList(processed.Count, yParam, MapParamType.Y);

                for (int i = 0; i <= processed.Count - 1; i++)
                {
                    dataTable.Rows.Add(processed[i].Select(x => x.ToString()).ToArray());
                    gridView.Rows[i].HeaderCell.Value = yParamData[i];
                }

                SetTableProperties();
                updateMapCellData();
                CheckCellHiLo();
                updateHeatMap();
                BuildData();
                tableReadyEdit = true;
            }
        }
        #endregion

        #region Buttons

        private void regenMapButton_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Are you sure you want to regenerate the table?\r\nThis action cannot be undone", "Regenerate Table", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                BuildTable();
            }
        }

        private void enableButton_Click(object sender, EventArgs e)
        {
            enable();
        }

        private void disableButton_Click(object sender, EventArgs e)
        {
            disable();
        }

        private void deleteMapButton_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Are you sure you want to delete this map?\r\nThis action cannot be undone", "Delete Map", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                Delete();
            }
        }
        
        private void btnAdjustTable_Click(object sender, EventArgs e)
        {
            var sel = gridView.SelectedCells;
            if(!enabled)
            {
                var frmGridSelectionAdj = new FrmAdjust(ref sel);
                var dialogResult = frmGridSelectionAdj.ShowDialog(mainForm);
                if ((uint)(dialogResult - 1) <= 1u)
                {
                    frmGridSelectionAdj.Dispose();
                }
            }
            else
            {
                MessageBox.Show(@"Please disable the map before adjusting the table", 
                    @"Map Enabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region HeatMap

        private void updateHeatMap()
        {
            Thread t = new Thread(() =>
            {
                gridView.Invoke((MethodInvoker)delegate
                {
                    var values = new List<double>();
                    foreach (DataGridViewRow row in gridView.Rows)
                    {
                        for (var i = 0; i < gridView.Columns.Count; i++)
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

                    foreach (DataGridViewRow row in gridView.Rows)
                    {
                        for (int i = 0; i < gridView.Columns.Count; i++)
                        {
                            DataGridViewCell cell = row.Cells[i];
                            double value = Convert.ToDouble(cell.Value);
                            if (max != min)
                            {
                                cell.Style.BackColor = HeatMap.GetColorForValue(value, max, min);
                            }
                            else
                            {
                                cell.Style.BackColor = Color.FromArgb(255, (byte)0, (byte)255, (byte)0);
                            }

                            if (Settings.Default.HeatFontAuto)
                            {
                                cell.Style.ForeColor = Helpers.GetReadableTextColor(cell.Style.BackColor);
                            } else
                            {
                                cell.Style.ForeColor = Settings.Default.HeatFontColour;
                            }
                        }
                    }

                });
                Debug.WriteLog("Refreshed Heat Map");
                Thread.Sleep(10);
            });
            t.Start();
        }

        private void CheckCellHiLo()
        {
            float? tentativeMin = null;
            float? tentativeMax = null;

            foreach (DataGridViewRow row in gridView.Rows)
            {
                foreach (DataGridViewCell c in row.Cells)
                {
                    if (float.TryParse(c.Value.ToString(), out float val))
                    {
                        tentativeMin = !tentativeMin.HasValue ? val : Math.Min(tentativeMin.Value, val);
                        tentativeMax = !tentativeMax.HasValue ? val : Math.Max(tentativeMax.Value, val);
                    }
                }
            }

            bool needToUpdateHeatMap = false;

            if (tentativeMin.HasValue && tentativeMin.Value != minCellValue)
            {
                minCellValue = tentativeMin.Value;
                needToUpdateHeatMap = true;
            }

            if (tentativeMax.HasValue && tentativeMax.Value != maxCellValue)
            {
                maxCellValue = tentativeMax.Value;
                needToUpdateHeatMap = true;
            }

            if (needToUpdateHeatMap)
            {
                updateHeatMap();
            }

            Debug.WriteLog($"Min: {minCellValue} Max: {maxCellValue}");
        }
        #endregion

        #region TableUpdates
        private void tableOverlayPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Pen p = new Pen(Color.Yellow, 3))
            {
                g.DrawEllipse(p, mapController.tablePoint);
            }
        }

        private void gridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (tableReadyEdit)
            {
                DataGridViewCell cell = gridView[e.ColumnIndex, e.RowIndex];
                float i = 0f;
                float.TryParse(cell.Value.ToString(), out i);

                if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()) || !Helpers.IsNumeric(cell.Value.ToString()))
                {
                    cell.Value = originalCellValue;
                    return;
                }

                CheckCellHiLo();
                updateHeatMap();
                BuildData();
                
            }
        }

        public void BuildData()
        {
            gridView.Invoke((MethodInvoker)delegate
            {
                List<MapCell> cellList = new List<MapCell>();
                foreach (DataGridViewRow row in gridView.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        float i = 0f;
                        float.TryParse(cell.Value.ToString(), out i);
                        MapCell sc = new MapCell();
                        Rectangle pos = gridView.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, false);
                        sc.Position = pos;
                        sc.Value = i;
                        cellList.Add(sc);
                    }
                }
                mapController.CacheData(cellList);
                Debug.WriteLog("Built map data");
            });
        }
        private int[] GetCells()
        {
            var selectedCells = gridView.SelectedCells;
            var column1 = 255;
            var column2 = 0;
            var row1 = 255;
            var row2 = 0;
            foreach (DataGridViewCell item in selectedCells)
            {
                if (item.ColumnIndex < column1)
                    column1 = item.ColumnIndex;
                if (item.RowIndex < row1)
                    row1 = item.RowIndex;
            }

            foreach (DataGridViewCell item2 in selectedCells)
            {   
                if (item2.ColumnIndex > column2)
                    column2 = item2.ColumnIndex;
                if (item2.RowIndex > row2)
                    row2 = item2.RowIndex;
            }

            return new int[4] { column1, row1, column2, row2 };
        }
        
        private void InterpolateRows(object sender, EventArgs e)
        {
            var cols1 = GetCells()[0]; // first column/row set to interpolate
            var rows1 = GetCells()[1];
            var cols2 = GetCells()[2]; // end column/row set to interpolate
            var rows2 = GetCells()[3];
            if (rows2 - rows1 < 2) return;
            var num = (float.Parse(gridView[cols1, rows2].Value.ToString()) -
                       float.Parse(gridView[cols1, rows1].Value.ToString()))
                      / (float.Parse(gridView.Rows[rows2].HeaderCell.Value.ToString()) -
                         float.Parse(gridView.Rows[rows1].HeaderCell.Value.ToString()));

            var num2 = float.Parse(gridView[cols1, rows1].Value.ToString()) -
                       num * float.Parse(gridView.Rows[rows1].HeaderCell.Value.ToString());

            for (var b5 = (byte)(rows1 + 1); b5 < rows2; b5 = (byte)(b5 + 1))
            {
                var @float = num * float.Parse(gridView.Rows[b5].HeaderCell.Value.ToString()) + num2;
                gridView[cols1, b5].Value = Math.Round(@float, 1);
            }

            if (cols1 >= cols2) return;
            num = (float.Parse(gridView[cols2, rows2].Value.ToString()) -
                   float.Parse(gridView[cols2, rows1].Value.ToString()))
                  / (float.Parse(gridView.Rows[rows2].HeaderCell.Value.ToString()) -
                     float.Parse(gridView.Rows[rows1].HeaderCell.Value.ToString()));

            num2 = float.Parse(gridView[cols2, rows1].Value.ToString()) -
                   num * float.Parse(gridView.Rows[rows1].HeaderCell.Value.ToString());
            for (var b6 = (byte)(rows1 + 1); b6 < rows2; b6 = (byte)(b6 + 1))
            {
                var float2 = num * float.Parse(gridView.Rows[b6].HeaderCell.Value.ToString()) + num2;
                gridView[cols2, b6].Value = Math.Round(float2, 1);
            }

        }
        
        private void InterpolateColumns(object sender, EventArgs e)
        {
            var cols1 = GetCells()[0]; // first column/row set to interpolate
            var rows1 = GetCells()[1];
            var cols2 = GetCells()[2]; // end column/row set to interpolate
            var rows2 = GetCells()[3];
            if (cols2 - cols1 < 2)
                return;
            
            for (var b5 = rows1; b5 <= rows2; b5++)
            {
                var num = (float.Parse(gridView[cols2, b5].Value.ToString()) - float.Parse(gridView[cols1, b5].Value.ToString()))
                          / (float.Parse(gridView.Columns[cols2].HeaderText) - float.Parse(gridView.Columns[cols1].HeaderText));
                var num2 = float.Parse(gridView[cols1, b5].Value.ToString()) -
                           num * float.Parse(gridView.Columns[cols1].HeaderText);

                for (var b6 = cols1 + 1; b6 < cols2; b6++)
                {
                    var @float = num * float.Parse(gridView.Columns[b6].HeaderText) + num2;
                    gridView[b6, b5].Value = Math.Round(@float, 1);
                }
            }
        }
        
        private void gridView_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.Control)
            {
                if (e.KeyCode == Keys.V)
                {
                    foreach (DataGridViewCell cell in gridView.SelectedCells)
                    {
                        cell.Value = Clipboard.GetText();
                    }
                    return;
                }
                if (e.KeyCode == Keys.C)
                {
                    DataGridViewCell c = gridView.SelectedCells[0];
                    Clipboard.SetText(c.Value.ToString());
                    return;
                }
            }
            else if(e.Alt)
            {
                if (e.KeyCode == Keys.V)
                {
                    if(gridView.SelectedCells.Count > 3)
                        InterpolateRows(null, null);
                }

                if (e.KeyCode == Keys.H)
                {
                    if(gridView.SelectedCells.Count > 3)
                        InterpolateColumns(null, null);
                }
            }
        }

        private void gridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            originalCellValue = gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void SetTableProperties()
        {
            foreach (DataGridViewColumn c in gridView.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
                c.Resizable = DataGridViewTriState.False;
            }

            foreach (DataGridViewRow r in gridView.Rows)
            {
                r.Resizable = DataGridViewTriState.False;
            }

            gridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }

        #endregion
    }
}

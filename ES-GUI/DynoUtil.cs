using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ES_GUI
{
    public class DynoPowerPeak
    {
        public double Value = 0;
        public double RPM = 0;
    }

    public class DynoUtil
    {
        private Dictionary<string, Panel> seriesPanels = new Dictionary<string, Panel>();
        private Chart dynoChart;

        private Label powerLabel;
        private Label powerLabelRPM;
        private Label torqueLabel;
        private Label torqueLabelRPM;

        public DynoUtil(Chart c, Label pLabel, Label pLabelRPM, Label tLabel, Label tLabelRPM)
        {
            dynoChart = c;
            powerLabel = pLabel;
            powerLabelRPM = pLabelRPM;
            torqueLabel = tLabel;
            torqueLabelRPM = tLabelRPM;
        }

        public void ConfigureDynoChart()
        {
            dynoChart.Series.Clear();
            dynoChart.ChartAreas.Clear();

            ChartArea dynoArea = new ChartArea();
            dynoArea.Name = "DynoArea";

            dynoChart.ChartAreas.Add(dynoArea);

            dynoArea.Position = new ElementPosition(0, 0, 90, 100);
            dynoArea.InnerPlotPosition = new ElementPosition(5, 5, 90, 90);

            dynoArea.AxisX.Title = "RPM";
            dynoArea.AxisX.Minimum = 0;
            dynoArea.AxisX.Maximum = 1000;

            dynoArea.AxisY.Title = "Data";
            dynoArea.AxisY.Minimum = 0;
            dynoArea.AxisY.Maximum = 100;

            Series hpSeries = new Series("Power");
            hpSeries.ChartType = SeriesChartType.Line;
            hpSeries.Color = Color.Red;
            hpSeries.ChartArea = "DynoArea";
            dynoChart.Series.Add(hpSeries);

            Series torqueSeries = new Series("Torque");
            torqueSeries.ChartType = SeriesChartType.Line;
            torqueSeries.Color = Color.Blue;
            torqueSeries.ChartArea = "DynoArea";
            dynoChart.Series.Add(torqueSeries);

            Series tpsSeries = new Series("TPS");
            tpsSeries.ChartType = SeriesChartType.Line;
            tpsSeries.Color = Color.SkyBlue;
            tpsSeries.ChartArea = "DynoArea";
            dynoChart.Series.Add(tpsSeries);

            Series sparkSeries = new Series("Spark");
            sparkSeries.ChartType = SeriesChartType.Line;
            sparkSeries.Color = Color.DarkGreen;
            sparkSeries.ChartArea = "DynoArea";
            dynoChart.Series.Add(sparkSeries);

            Series airSeries = new Series("Air");
            airSeries.ChartType = SeriesChartType.Line;
            airSeries.Color = Color.Orange;
            airSeries.ChartArea = "DynoArea";
            dynoChart.Series.Add(airSeries);

            dynoChart.ChartAreas["DynoArea"].AxisX.MajorGrid.Enabled = true;
            dynoChart.ChartAreas["DynoArea"].AxisX.MajorGrid.LineColor = Color.LightGray;
            dynoChart.ChartAreas["DynoArea"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            dynoChart.ChartAreas["DynoArea"].AxisX.MajorGrid.LineWidth = 1;

            dynoChart.ChartAreas["DynoArea"].AxisY.MajorGrid.Enabled = true;
            dynoChart.ChartAreas["DynoArea"].AxisY.MajorGrid.LineColor = Color.LightGray;
            dynoChart.ChartAreas["DynoArea"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            dynoChart.ChartAreas["DynoArea"].AxisY.MajorGrid.LineWidth = 1;

            dynoChart.ChartAreas["DynoArea"].CursorX.IsUserEnabled = true;
            dynoChart.ChartAreas["DynoArea"].CursorX.IsUserSelectionEnabled = true;
            dynoChart.ChartAreas["DynoArea"].CursorX.LineColor = Color.Gray;
            dynoChart.ChartAreas["DynoArea"].CursorX.LineWidth = 1;
            dynoChart.ChartAreas["DynoArea"].CursorX.LineDashStyle = ChartDashStyle.Dot;

            ResetLabels();

            ThemeManager.ApplyTheme(dynoChart);
        }

        public void AddDataToChart(double rpm, double torque, double horsepower, double tps, double spark, double air)
        {
            DataPointCollection hpPoints = dynoChart.Series["Power"].Points;
            if (hpPoints.Count > 0 && rpm <= hpPoints[hpPoints.Count - 1].XValue)
                return;

            dynoChart.Series["Power"].Points.AddXY(rpm, horsepower);
            dynoChart.Series["Torque"].Points.AddXY(rpm, torque);
            dynoChart.Series["TPS"].Points.AddXY(rpm, tps);
            dynoChart.Series["Spark"].Points.AddXY(rpm, spark);

            if (dynoChart.Series["Air"].Points.Any())
            {
                if (air > (dynoChart.Series["Air"].Points.Last().YValues.Last() - 50))
                {
                    dynoChart.Series["Air"].Points.AddXY(rpm, air);
                    ApplyMovingAverageToSeries(dynoChart.Series["Air"], 2);
                    dynoChart.Invalidate();
                }
            }
            else
            {
                dynoChart.Series["Air"].Points.AddXY(rpm, air);
            }

            if (rpm > dynoChart.ChartAreas["DynoArea"].AxisX.Maximum)
            {
                double adjustedRPM = Math.Ceiling((rpm + 100) / 500) * 500;
                dynoChart.ChartAreas["DynoArea"].AxisX.Maximum = adjustedRPM;
            }

            double newMaxSCFM = Math.Ceiling((air + 5) / 10) * 10;
            if (newMaxSCFM > dynoChart.ChartAreas["DynoArea"].AxisY.Maximum)
            {
                dynoChart.ChartAreas["DynoArea"].AxisY.Maximum = newMaxSCFM;
            }

            double newMaxHorsepower = Math.Ceiling((horsepower + 5) / 10) * 10;
            if (newMaxHorsepower > dynoChart.ChartAreas["DynoArea"].AxisY.Maximum)
            {
                dynoChart.ChartAreas["DynoArea"].AxisY.Maximum = newMaxHorsepower;
            }

            double newMaxTorque = Math.Ceiling((torque + 5) / 10) * 10;
            if (newMaxTorque > dynoChart.ChartAreas["DynoArea"].AxisY.Maximum)
            {
                dynoChart.ChartAreas["DynoArea"].AxisY.Maximum = newMaxTorque;
            }

            UpdateLabels();
        }

        public void CreateDynoLabels()
        {
            seriesPanels.Clear();
            foreach (Series s in dynoChart.Series)
            {
                Panel p = new Panel
                {
                    Size = new Size(100, 25),
                    BackColor = Color.FromArgb(150, 255, 255, 255),
                    Visible = false
                };

                Label l = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                p.Controls.Add(l);

                dynoChart.Controls.Add(p);
                seriesPanels[s.Name] = p;
            }
            ThemeManager.ApplyTheme(dynoChart);
        }

        public void dynoChart_MouseEnter(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, Panel> s in seriesPanels)
            {
                s.Value.Visible = true;
            }
        }

        public void dynoChart_MouseLeave(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, Panel> s in seriesPanels)
            {
                s.Value.Visible = false;
            }
        }

        public void dynoChart_MouseMove(object sender, MouseEventArgs e)
        {
            double mouseX;
            try
            {
                mouseX = dynoChart.ChartAreas["DynoArea"].AxisX.PixelPositionToValue(e.Location.X);
            }
            catch
            {
                foreach (var panel in seriesPanels.Values)
                {
                    panel.Visible = false;
                }
                return;
            }

            dynoChart.ChartAreas["DynoArea"].CursorX.Position = mouseX;

            foreach (Series s in dynoChart.Series)
            {
                if (!s.Enabled)
                {
                    seriesPanels[s.Name].Visible = false;
                    continue;
                }

                DataPoint closestDataPoint = s.Points.OrderBy(p => Math.Abs(p.XValue - mouseX)).FirstOrDefault();

                if (closestDataPoint != null)
                {
                    Panel currentPanel = seriesPanels[s.Name];
                    Label labelInPanel = (Label)currentPanel.Controls[0];

                    labelInPanel.Text = $"{s.Name}: {closestDataPoint.YValues[0]:F2}";
                    labelInPanel.AutoSize = true;
                    currentPanel.Size = new Size(labelInPanel.Width + 10, labelInPanel.Height + 5);

                    int panelX = (int)dynoChart.ChartAreas["DynoArea"].AxisX.ValueToPixelPosition(closestDataPoint.XValue);
                    int panelY = (int)dynoChart.ChartAreas["DynoArea"].AxisY.ValueToPixelPosition(closestDataPoint.YValues[0]);

                    foreach (Series otherSeries in dynoChart.Series)
                    {
                        if (otherSeries != s && otherSeries.Enabled)
                        {
                            DataPoint otherDataPoint = otherSeries.Points.OrderBy(p => Math.Abs(p.XValue - mouseX)).FirstOrDefault();
                            if (otherDataPoint != null)
                            {
                                int otherPanelY = (int)dynoChart.ChartAreas["DynoArea"].AxisY.ValueToPixelPosition(otherDataPoint.YValues[0]);
                                int distanceBetweenPanels = Math.Abs(panelY - otherPanelY);
                                int requiredDistance = currentPanel.Height + 8;

                                if (distanceBetweenPanels < requiredDistance)
                                {
                                    int adjustment = (requiredDistance - distanceBetweenPanels) / 2;

                                    if (closestDataPoint.YValues[0] > otherDataPoint.YValues[0])
                                    {
                                        panelY -= adjustment;
                                    }
                                    else
                                    {
                                        panelY += adjustment;
                                    }
                                }
                            }
                        }
                    }

                    currentPanel.Location = new Point(panelX - currentPanel.Width / 2, panelY - currentPanel.Height / 2);
                    currentPanel.Visible = true;
                }
                else
                {
                    seriesPanels[s.Name].Visible = false;
                }
            }
        }

        public void dynoChart_MouseHover(object sender, EventArgs e)
        {
            //
        }

        public List<double> MovingAverage(List<double> values, int windowSize)
        {
            var result = new List<double>();

            for (int i = 0; i < values.Count; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(values.Count - 1, i + windowSize / 2);
                double average = 0;

                for (int j = start; j <= end; j++)
                {
                    average += values[j];
                }

                average /= (end - start + 1);
                result.Add(average);
            }

            return result;
        }

        public void ApplyMovingAverageToSeries(Series series, int windowSize)
        {
            var yValues = series.Points.Select(p => p.YValues[0]).ToList();

            var smoothedValues = MovingAverage(yValues, windowSize);

            for (int i = 0; i < series.Points.Count; i++)
            {
                series.Points[i].YValues[0] = smoothedValues[i];
            }
        }

        public void SmoothAllSeries(int windowSize)
        {
            foreach (Series s in dynoChart.Series)
            {
                ApplyMovingAverageToSeries(s, windowSize);
            }

            dynoChart.Invalidate();
        }

        public void ToggleAdvancedInfo(bool showAdvanced)
        {
            double maxYValue = 0;

            foreach (Series s in dynoChart.Series)
            {
                if (s.Name != "Power" && s.Name != "Torque")
                {
                    s.Enabled = showAdvanced;
                }

                if (s.Enabled)
                {
                    foreach (DataPoint point in s.Points)
                    {
                        if (point.YValues[0] > maxYValue)
                        {
                            maxYValue = point.YValues[0];
                        }
                    }
                }
            }

            double newMaxY = Math.Ceiling((maxYValue + 5) / 10) * 10;

            if (newMaxY > dynoChart.ChartAreas["DynoArea"].AxisY.Maximum)
            {
                dynoChart.ChartAreas["DynoArea"].AxisY.Maximum = newMaxY;
            }
            else if (showAdvanced == false)
            {
                dynoChart.ChartAreas["DynoArea"].AxisY.Maximum = newMaxY;
            }

            dynoChart.Invalidate();
        }

        private DynoPowerPeak getMaxPower()
        {
            DynoPowerPeak maxPower = new DynoPowerPeak();

            foreach (DataPoint point in dynoChart.Series["Power"].Points)
            {
                if (point.YValues[0] > maxPower.Value)
                {
                    maxPower.Value = point.YValues[0];
                    maxPower.RPM = point.XValue;
                }
            }

            return maxPower;
        }

        private DynoPowerPeak getMaxTorque()
        {
            DynoPowerPeak maxTorque = new DynoPowerPeak();

            foreach (DataPoint point in dynoChart.Series["Torque"].Points)
            {
                if (point.YValues[0] > maxTorque.Value)
                {
                    maxTorque.Value = point.YValues[0];
                    maxTorque.RPM = point.XValue;
                }
            }

            return maxTorque;
        }

        private void UpdateLabels()
        {
            DynoPowerPeak maxPower = getMaxPower();
            DynoPowerPeak maxTorque = getMaxTorque();

            powerLabel.Text = Math.Round(maxPower.Value, 2).ToString();
            powerLabelRPM.Text = Math.Round(maxPower.RPM).ToString() + " RPM";

            torqueLabel.Text = Math.Round(maxTorque.Value, 2).ToString();
            torqueLabelRPM.Text = Math.Round(maxTorque.RPM).ToString() + " RPM";
        }

        private void ResetLabels()
        {
            powerLabel.Text = "0.00";
            powerLabelRPM.Text = "0 RPM";

            torqueLabel.Text = "0.00";
            torqueLabelRPM.Text = "0 RPM";
        }
    }
}

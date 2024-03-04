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
    public class DynoUtil
    {
        private Dictionary<string, Panel> seriesPanels = new Dictionary<string, Panel>();
        private Chart dynoChart;

        public DynoUtil(Chart c) 
        {
            dynoChart = c;
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
            tpsSeries.BorderDashStyle = ChartDashStyle.Dot;
            dynoChart.Series.Add(tpsSeries);

            Series sparkSeries = new Series("Spark");
            sparkSeries.ChartType = SeriesChartType.Line;
            sparkSeries.Color = Color.DarkGreen;
            sparkSeries.ChartArea = "DynoArea";
            sparkSeries.BorderDashStyle = ChartDashStyle.Dot;
            dynoChart.Series.Add(sparkSeries);

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

            ThemeManager.ApplyTheme(dynoChart);
        }

        public void AddDataToChart(double rpm, double torque, double horsepower, double tps, double spark)
        {
            DataPointCollection hpPoints = dynoChart.Series["Power"].Points;
            if (hpPoints.Count > 0 && rpm <= hpPoints[hpPoints.Count - 1].XValue)
                return;

            dynoChart.Series["Power"].Points.AddXY(rpm, horsepower);
            dynoChart.Series["Torque"].Points.AddXY(rpm, torque);
            dynoChart.Series["TPS"].Points.AddXY(rpm, tps);
            dynoChart.Series["Spark"].Points.AddXY(rpm, spark);

            if (rpm > dynoChart.ChartAreas["DynoArea"].AxisX.Maximum)
            {
                double adjustedRPM = Math.Ceiling((rpm + 100) / 500) * 500;
                dynoChart.ChartAreas["DynoArea"].AxisX.Maximum = adjustedRPM;
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
        }

        public void CreateDynoLabels()
        {
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
            double mouseX = dynoChart.ChartAreas["DynoArea"].AxisX.PixelPositionToValue(e.Location.X);
            dynoChart.ChartAreas["DynoArea"].CursorX.Position = mouseX;

            int panelSpacing = 8;

            foreach (Series s in dynoChart.Series)
            {
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
                        if (otherSeries != s)
                        {
                            DataPoint otherDataPoint = otherSeries.Points.OrderBy(p => Math.Abs(p.XValue - mouseX)).FirstOrDefault();
                            if (otherDataPoint != null)
                            {
                                int otherPanelY = (int)dynoChart.ChartAreas["DynoArea"].AxisY.ValueToPixelPosition(otherDataPoint.YValues[0]);
                                int distanceBetweenPanels = Math.Abs(panelY - otherPanelY);
                                int requiredDistance = currentPanel.Height + panelSpacing;

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
    }
}

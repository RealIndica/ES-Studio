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

namespace ES_GUI
{
    public partial class Form1 : Form
    {
        private ESClient client;

        private List<double> timingList;

        private PictureBox sparkTableOverlay;
        private Stopwatch sparkOverlayTimer;

        private List<Thread> threadPool;

        private bool ignReadyEdit = false;

        private Stopwatch powerBuilderTimer;

        private PowerBuilder powerBuilder;

        public Form1()
        {
            InitializeComponent();
            positionAsset(powerBuilderRev1, powerBuilderPicture, true);
            positionAsset(powerBuilderRev2, powerBuilderPicture, true);
            positionAsset(powerBuilderGain, powerBuilderPicture, true);
            positionAsset(ledOffPicture, powerBuilderPicture, true);
            positionAsset(ledOnPicture, powerBuilderPicture, true, ledOffPicture);

            ThemeManager.darkMode = true;
            ThemeManager.ApplyTheme(this);
        }

        //This is so hacky omg winforms suck
        private void positionAsset(Control asset, Control baseAsset, bool bringToFront = false, Control bindAsset = null, bool visible = true)
        {
            asset.BackColor = Color.Transparent;

            if (bindAsset != null)
            {
                Point center = baseAsset.PointToClient(bindAsset.PointToScreen(new Point((bindAsset.Width / 2) - (bindAsset.Width / 2), (bindAsset.Height / 2) - (bindAsset.Height / 2))));
                center.X -= asset.Width / 2;
                center.Y -= asset.Height / 2;
                center.X += bindAsset.Width / 2;
                center.Y += bindAsset.Height / 2;
                asset.Location = center;
            } else
            {
                Point pos = baseAsset.PointToClient(asset.Parent.PointToScreen(asset.Location));
                asset.Location = pos;
            }

            if (bringToFront)
            {
                asset.BringToFront();
            }

            asset.Visible = visible;
            asset.Parent = baseAsset;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                manageControls(true);
                manageModules(false);
                this.Text += " - UI Preview Mode";
            }
            else
            {
                if (client.Connect())
                {
                    ClientUpdate();
                    manageControls(true);
                    manageModules(false);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new ESClient();
            powerBuilder = new PowerBuilder(client);

            timingList = new List<double>();
            threadPool = new List<Thread>();

            sparkTable.DataSource = client.timingTable;
            sparkOverlayTimer = new Stopwatch();

            sparkTableOverlay = new PictureBox();
            sparkTableOverlay.Parent = null;
            sparkTableOverlay.Size = new Size(0, 0);
            sparkTableOverlay.Location = new Point(0, 0);
            sparkTableOverlay.BackColor = Color.Transparent;
            sparkTableOverlay.Paint += new PaintEventHandler(sparkTableOverlayPaint);

            powerBuilderTimer = new Stopwatch();

            loadSourceBox.Items.AddRange(new string[] { "Manifold Pressure", "Throttle Position" });
            loadSourceBox.SelectedIndex = 0;

            quickShifterMode.Items.AddRange(new string[] { "Cut Ignition", "Retard Ignition" });
            quickShifterMode.SelectedIndex = 0;

            twoStepSwitch.Items.AddRange(new string[] { "Clutch" });
            twoStepSwitch.SelectedIndex = 0;

            twoStepLimiterModeBox.Items.AddRange(new string[] { "SOFT CUT", "HARD CUT" , "RETARD" });
            twoStepLimiterModeBox.SelectedIndex = 0;

            ledOffPicture.Visible = true;
            ledOnPicture.Visible = false;

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            manageControls(false);
            GaugeSweep();
        }

        private void killAllThreads()
        {
            foreach (Thread t in threadPool)
            {
                if (t.IsAlive)
                {
                    t.Abort();
                }
            }
            threadPool.Clear();
        }

        private void manageControls(bool enabled, Control.ControlCollection controls = null)
        {
            if (controls == null)
            {
                controls = this.Controls;
            }
            foreach (Control control in controls)
            {
                if (control.GetType() != typeof(ToolStrip) && control.GetType() != typeof(Label))
                {
                    control.Enabled = enabled;
                    if (control.HasChildren)
                    {
                        manageControls(enabled, control.Controls);
                    }
                }
            }
        }

        private void manageModules(bool enabled)
        {
            Control tab = tabPage4;
            foreach (Control c in tab.Controls)
            {
                c.Enabled = enabled;
            }
            tab.Controls.Find("ignitionModuleMaster", true)[0].Enabled = true;
        }

        private void GaugeSweep()
        {
            Thread t = new Thread(() =>
            {
                for (int i = 1; i <= 100; i++)
                {
                    foreach (Control c in tabPage1.Controls)
                    {
                        if (c.GetType() == typeof(AquaGauge))
                        {
                            AquaGauge g = (AquaGauge)c;
                            g.Invoke((MethodInvoker)delegate
                            {
                                g.Value = (g.MaxValue / 100) * i;
                            });
                        }
                    }
                    Thread.Sleep(2);
                }
                Thread.Sleep(100);
                for (int i = 100; i >= 1; i--)
                {
                    foreach (Control c in tabPage1.Controls)
                    {
                        if (c.GetType() == typeof(AquaGauge))
                        {
                            AquaGauge g = (AquaGauge)c;
                            g.Invoke((MethodInvoker)delegate
                            {
                                g.Value = (g.MaxValue / 100) * i;
                            });
                        }
                    }
                    Thread.Sleep(2);
                }
                foreach (Control c in tabPage1.Controls)
                {
                    if (c.GetType() == typeof(AquaGauge))
                    {
                        AquaGauge g = (AquaGauge)c;
                        g.Invoke((MethodInvoker)delegate
                        {
                            g.Value = 0f;
                        });
                    }
                }
            });
            t.Start();
            threadPool.Add(t);
        }

        private Color HeatMap(float value, float max)
        {
            if (value < 0)
            {
                return Color.FromArgb(255, 0, 0, 255);
            }
            int r, g, b;
            float val = value / max;
            if (val > 1)
                val = 1;
            if (val > 0.5f)
            {
                val = (val - 0.5f) * 2;
                r = Convert.ToByte(255 * val);
                g = Convert.ToByte(255 * (1 - val));
                b = 0;
            }
            else
            {
                val = val * 2;
                r = 0;
                g = Convert.ToByte(255 * val);
                b = Convert.ToByte(255 * (1 - val));
            }
            return Color.FromArgb(255, r, g, b);
        }

        private void updateHeatMap(DataGridView dgv)
        {
            Thread t = new Thread(() =>
            {
                dgv.Invoke((MethodInvoker)delegate
                {
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            float i = 0f;
                            float.TryParse(cell.Value.ToString(), out i);
                            Color col = HeatMap(i, 40f);
                            DataGridViewCellStyle style = new DataGridViewCellStyle();
                            style.BackColor = col;
                            style.ForeColor = Color.Black;
                            style.Font = new Font(Font.Name, Font.Size, FontStyle.Bold);
                            cell.Style = style;
                        }
                    }
                });
                Thread.Sleep(10);
            });
            t.Start();
        }

        private void BuildSparkData()
        {
            sparkTable.Invoke((MethodInvoker)delegate
            {
                List<SparkCell> cellList = new List<SparkCell>();
                foreach (DataGridViewRow row in sparkTable.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        float i = 0f;
                        float.TryParse(cell.Value.ToString(), out i);
                        SparkCell sc = new SparkCell();
                        Rectangle pos = sparkTable.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, false);
                        sc.Position = pos;
                        sc.Advance = i;
                        cellList.Add(sc);
                    }
                }
                client.ignController.CacheSparkData(cellList);
            });
        }

        private void BuildSparkTable()
        {
            if (client.isConnected)
            {
                ignReadyEdit = false;
                client.timingTable.Clear();
                client.timingTable = new DataTable();
                sparkTable.DataSource = client.timingTable;

                timingList = client.update.sparkTimingList;

                for (int i = 0; i < timingList.Count; i++)
                {
                    client.timingTable.Columns.Add((500 * i).ToString());
                }

                List<double> last = new List<double>();
                last.AddRange(timingList);

                double offset = 1;
                double min = last.OrderBy(x => x).ElementAt(1);

                List<List<double>> processed = new List<List<double>>();

                for (int i = 0; i <= 20; i++)
                {
                    List<double> t = new List<double>();

                    if (i == 0)
                    {
                        processed.Add(timingList);
                        continue;
                    }

                    for (int s = 0; s < last.Count; s++)
                    {
                        if (s != 0)
                            t.Add((last[s] - (offset)).Clamp(min, 100000f));
                        else
                            t.Add(last[s]);
                    }

                    processed.Add(t);
                    last.Clear();
                    last.AddRange(t);
                }

                processed.Reverse();

                for (int i = 0; i <= 20; i++)
                {
                    client.timingTable.Rows.Add(processed[i].Select(x => x.ToString()).ToArray());
                    sparkTable.Rows[i].HeaderCell.Value = (i * 5).ToString();
                }

                client.ignController.cellRectangle = sparkTable.GetCellDisplayRectangle(sparkTable.ColumnCount - 1, sparkTable.RowCount - 1, false);
                client.ignController.cellOffset.X = sparkTable.Rows[0].HeaderCell.Size.Width;
                client.ignController.cellOffset.Y = sparkTable.Columns[0].HeaderCell.Size.Height;
                client.ignController.sparkTablePoint.Width = client.ignController.cellRectangle.Width;
                client.ignController.sparkTablePoint.Height = client.ignController.cellRectangle.Height;

                updateHeatMap(sparkTable);
                BuildSparkData();
                ignReadyEdit = true;
            }
        }

        private void ClientUpdate()
        {
            Thread t = new Thread(() =>
            {
            double divider = 100d;

                while (true)
                {
                    if (client.isConnected)
                    {
                        client.onUpdate();

                        client.ignController.UpdateSparkTablePos(client);

                        if (!sparkOverlayTimer.IsRunning)
                        {
                            sparkOverlayTimer.Start();
                        }
                        else
                        {
                            if (sparkOverlayTimer.ElapsedMilliseconds > 50)
                            {
                                sparkOverlayTimer.Stop();
                                sparkOverlayTimer.Reset();
                                sparkTableOverlay.Invalidate();
                            }
                        }

                        if (!powerBuilderTimer.IsRunning)
                        {
                            powerBuilderTimer.Start();
                        }

                        if (client.edit.useIgnTable)
                        {
                            client.edit.customSpark = client.ignController.Pos2Spark();
                        }

                        rpmGauge.Invoke((MethodInvoker)delegate
                        {
                            if (rpmX10.Checked)
                                divider = 10d;
                            if (rpmX100.Checked)
                                divider = 100d;
                            if (rpmX1000.Checked)
                                divider = 1000d;

                            double RPM = client.update.RPM;

                            if (rpmSmoothing.Checked)
                            {
                                RPM = client.smoothRPM;
                            }

                            rpmGauge.DialText = "RPM x" + divider.ToString();
                            double result = client.update.maxRPM % 1000d >= 500d ? client.update.maxRPM + 1000d - client.update.maxRPM % 1000d : client.update.maxRPM - client.update.maxRPM % 1000d;
                            rpmGauge.MaxValue = (float)(result / divider);
                            rpmGauge.NoOfDivisions = Convert.ToInt32(result / 1000d);
                            rpmGauge.Value = (float)(RPM / divider);
                        });

                        limiterLight.Invoke((MethodInvoker)delegate
                        {
                            if (client.update.atLimiter)
                            {
                                limiterLight.ForeColor = Color.Red;
                            }
                            else
                            {
                                limiterLight.ForeColor = Color.Gray;
                            }
                        });

                        twoStepLight.Invoke((MethodInvoker)delegate
                        {
                            if (client.update.twoStepActive)
                            {
                                twoStepLight.ForeColor = Color.Red;
                            }
                            else
                            {
                                twoStepLight.ForeColor = Color.Gray;
                            }
                        });

                        tpsGauge.Invoke((MethodInvoker)delegate
                        {
                            tpsGauge.Value = (float)(client.update.tps * 100f);
                            tpsGauge.MaxValue = 100;
                        });

                        timingGauge.Invoke((MethodInvoker)delegate
                        {
                            timingGauge.Value = ((float)client.update.sparkAdvance).Clamp(-100, 100);
                            timingGauge.MaxValue = 100;
                        });

                        speedGauge.Invoke((MethodInvoker)delegate
                        {
                            double adjustedSpeed = (double)client.update.vehicleSpeed;

                            if (speedKmhButton.Checked)
                            {
                                adjustedSpeed /= (1000.0 / (60 * 60));
                                speedGauge.DialText = "KM/H";
                                speedGauge.MaxValue = 300;
                            }
                            else if (speedMphButton.Checked)
                            {
                                adjustedSpeed /= (1609.344 / (60 * 60));
                                speedGauge.DialText = "MPH";
                                speedGauge.MaxValue = 200;
                            }

                            speedGauge.Value = (float)adjustedSpeed;
                        });

                        engineNameLabel.Invoke((MethodInvoker)delegate
                        {
                            engineNameLabel.Text = "Name : " + client.update.Name;
                        });

                        engineRedlineLabel.Invoke((MethodInvoker)delegate
                        {
                            engineRedlineLabel.Text = "Redline : " + client.update.maxRPM.ToString() + " RPM";
                        });

                        engineCylinderCountLabel.Invoke((MethodInvoker)delegate
                        {
                            engineCylinderCountLabel.Text = "Cylinders : " + client.update.cylinderCount.ToString();
                        });

                        statusLabel.Invoke((MethodInvoker)delegate
                        {
                            statusLabel.Text = client.status;
                        });

                        gearLabel.Invoke((MethodInvoker)delegate
                        {
                            string Gear = "N";
                            if (client.update.gear == -1)
                            {
                                Gear = "N";
                            }
                            else
                            {
                                Gear = (client.update.gear + 1).ToString();
                            }
                            gearLabel.Text = Gear;
                        });

                        if (client.edit.idleHelper)
                        {
                            idleControlTPS.Invoke((MethodInvoker)delegate
                            {
                                idleControlTPS.Text = client.update.tps.ToString();
                            });
                        }

                        if (powerBuilderTimer.ElapsedMilliseconds > 50)
                        {

                            ledOffPicture.Invoke((MethodInvoker)delegate
                            {
                                ledOffPicture.Visible = !client.update.atLimiter;
                            });

                            ledOnPicture.Invoke((MethodInvoker)delegate
                            {
                                ledOnPicture.Visible = client.update.atLimiter;
                            });

                            powerBuilderTimer.Stop();
                            powerBuilderTimer.Reset();
                        }
                    } 
                    else
                    {
                        manageControls(false);
                        tabControl1.SelectedIndex = 0;
                        break;
                    }
                }
            });
            t.Start();
            threadPool.Add(t);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            killAllThreads();
            Environment.Exit(0);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (client.isConnected)
            {
                sparkAdvanceLabel.Text = trackBar1.Value.ToString();
                client.edit.sparkAdvance = trackBar1.Value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trackBar1.Value = 0;
            if (client.isConnected)
            {
                sparkAdvanceLabel.Text = trackBar1.Value.ToString();
                client.edit.sparkAdvance = trackBar1.Value;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BuildSparkTable();
        }

        private void sparkTableOverlayPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Pen p = new Pen(Color.Yellow, 3))
            {
                g.DrawEllipse(p, client.ignController.sparkTablePoint);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                client.edit.useIgnTable = true;
                sparkTableOverlay.Parent = sparkTable;
                sparkTableOverlay.Size = sparkTable.Size;
            } else
            {
                client.edit.useIgnTable = false;
                sparkTableOverlay.Parent = null;
                sparkTableOverlay.Size = new Size(0, 0);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox2.Checked;
        }

        private void sparkTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (ignReadyEdit)
            {
                DataGridViewCell cell = sparkTable[e.ColumnIndex, e.RowIndex];
                float i = 0f;
                float.TryParse(cell.Value.ToString(), out i);
                Color col = HeatMap(i, 40f);
                DataGridViewCellStyle style = new DataGridViewCellStyle();
                style.BackColor = col;
                style.ForeColor = Color.Black;
                style.Font = new Font(Font.Name, Font.Size, FontStyle.Bold);
                cell.Style = style;
                BuildSparkData();
            }
        }

        private void loadSourceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            client.ignController.loadSource = loadSourceBox.Text;
        }

        private void quickShiftTimeBox_TextChanged(object sender, EventArgs e)
        {
            client.edit.quickShiftTime = quickShiftTimeBox.Text.ToDouble() / 1000;
        }

        private void quickShiftTimeBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '-') && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            if ((e.KeyChar == '-') && ((sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }

            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > 0))
            {
                e.Handled = true;
            }
        }

        private void quickShifterEnabled_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.quickShiftEnabled = quickShifterEnabled.Checked;
            if (client.edit.quickShiftEnabled)
            {
                client.edit.quickShiftTime = quickShiftTimeBox.Text.ToDouble() / 1000;
                client.edit.quickShiftRetardTime = quickShiftRetardTimeBox.Text.ToDouble() / 1000;
                client.edit.quickShiftRetardDeg = quickShiftRetardDegBox.Text.ToDouble();
                client.edit.quickShiftAutoClutch = quickShiftAutoClutch.Checked;
                client.edit.quickShiftCutThenShift = quickShiftOrder1.Checked;
                client.edit.dsgFarts = quickShiftSimulateDSG.Checked;
            }
        }

        private void quickShiftRetardTimeBox_TextChanged(object sender, EventArgs e)
        {
            client.edit.quickShiftRetardTime = quickShiftRetardTimeBox.Text.ToDouble() / 1000;
        }

        private void quickShiftRetardDegBox_TextChanged(object sender, EventArgs e)
        {
            client.edit.quickShiftRetardDeg = quickShiftRetardDegBox.Text.ToDouble();
        }

        private void quickShiftSimulateDSG_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.dsgFarts = quickShiftSimulateDSG.Checked;
        }

        private void quickShifterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            client.edit.quickShiftMode = quickShifterMode.SelectedIndex;
        }

        private void quickShiftAutoClutch_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.quickShiftAutoClutch = quickShiftAutoClutch.Checked;
        }

        private void quickShiftOrder1_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.quickShiftCutThenShift = quickShiftOrder1.Checked;
        }

        private void autoBlipperEnabled_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.autoBlipEnabled = autoBlipperEnabled.Checked;
            client.edit.autoBlipThrottle = (double)trackBar2.Value / 100d;
            client.edit.autoBlipTime = autoBlipTimeBox.Text.ToDouble() / 1000;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            client.edit.autoBlipThrottle = (double)trackBar2.Value / 100d;
            label10.Text = trackBar2.Value.ToString() + "%";
        }

        private void autoBlipTimeBox_TextChanged(object sender, EventArgs e)
        {
            client.edit.autoBlipTime = autoBlipTimeBox.Text.ToDouble() / 1000;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            client.ignController.pressureLoadOffsetMin = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            client.ignController.pressureLoadOffsetMax = (int)numericUpDown2.Value;
        }

        private void initTwoStep()
        {
            client.edit.disableRevLimit = disableRevLimit.Checked;
            client.edit.rev1 = rev1Box.Text.ToDouble();
            client.edit.rev2 = rev2Box.Text.ToDouble();
            client.edit.twoStepLimiterMode = twoStepLimiterModeBox.SelectedIndex;
            client.edit.twoStepCutTime = twoStepCutTimeBox.Text.ToDouble();
            client.edit.twoStepRetardDeg = twoStepIgnitionRetardBox.Text.ToDouble();
            client.edit.twoStepSwitchThreshold = (double)trackBar3.Value / 100;
            client.edit.allowTwoStepInGear = checkBox4.Checked;
        }

        private void twoStepEnabled_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.twoStepEnabled = twoStepEnabled.Checked;
            if (client.edit.twoStepEnabled)
            {
                initTwoStep();
            }
        }

        private void disableRevLimit_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.disableRevLimit = disableRevLimit.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            manageModules(checkBox3.Checked);
            client.edit.useCustomIgnitionModule = checkBox3.Checked;
        }

        private void rev1Box_TextChanged(object sender, EventArgs e)
        {
            client.edit.rev1 = rev1Box.Text.ToDouble();
        }

        private void rev2Box_TextChanged(object sender, EventArgs e)
        {
            client.edit.rev2 = rev2Box.Text.ToDouble();
        }

        private void twoStepLimiterModeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            client.edit.twoStepLimiterMode = twoStepLimiterModeBox.SelectedIndex;
        }

        private void twoStepCutTimeBox_TextChanged(object sender, EventArgs e)
        {
            client.edit.twoStepCutTime = twoStepCutTimeBox.Text.ToDouble();
        }

        private void twoStepIgnitionRetardBox_TextChanged(object sender, EventArgs e)
        {
            client.edit.twoStepRetardDeg = twoStepIgnitionRetardBox.Text.ToDouble();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            switchThresholdLabel.Text = trackBar3.Value.ToString() + "%";
            client.edit.twoStepSwitchThreshold = (double)trackBar3.Value / 100;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.allowTwoStepInGear = checkBox4.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start("https://prjktmayhem.files.wordpress.com/2015/08/bee-r-rev-limiter-fitting-guide-english.pdf");
        }

        private void basicTwoStepActive_CheckedChanged(object sender, EventArgs e)
        {
            if (basicTwoStepActive.Checked)
            {
                powerBuilderTwoStepActive.Checked = false;
                initTwoStep();
            } else
            {
                powerBuilderTwoStepActive.Checked = true;
                powerBuilder.update();
            }
        }

        private void powerBuilderTwoStepActive_CheckedChanged(object sender, EventArgs e)
        {
            if (powerBuilderTwoStepActive.Checked)
            {
                basicTwoStepActive.Checked = false;
                powerBuilder.update();
            } else
            {
                basicTwoStepActive.Checked = true;
                initTwoStep();
            }
        }

        private void powerBuilderRev1_ValueChanged(object Sender)
        {
            powerBuilder.Rev1 = powerBuilderRev1.Value;
            if (powerBuilderTwoStepActive.Checked)
            {
                powerBuilder.update();
            }
        }

        private void powerBuilderRev2_ValueChanged(object Sender)
        {
            powerBuilder.Rev2 = powerBuilderRev2.Value;
            if (powerBuilderTwoStepActive.Checked)
            {
                powerBuilder.update();
            }
        }

        private void powerBuilderGain_ValueChanged(object Sender)
        {
            powerBuilder.Gain = powerBuilderGain.Value;
            if (powerBuilderTwoStepActive.Checked)
            {
                powerBuilder.update();
            }
        }

        private void idleControlEnabled_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.idleHelper = idleControlEnabled.Checked;
            if (idleControlEnabled.Checked)
            {
                client.edit.idleHelperRPM = idleControlTarget.Text.ToDouble();
            }
        }

        private void sparkTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.V)
                {
                    foreach (DataGridViewCell cell in sparkTable.SelectedCells)
                    {
                        cell.Value = Clipboard.GetText();
                    }
                    return;
                }
                if (e.KeyCode == Keys.C)
                {
                    DataGridViewCell c = sparkTable.SelectedCells[0];
                    Clipboard.SetText(c.Value.ToString());
                    return;
                }
            }
        }

        private void speedLimiterEnabled_CheckedChanged(object sender, EventArgs e)
        {
            client.edit.speedLimiter = speedLimiterEnabled.Checked;
            if (speedLimiterEnabled.Checked)
            {
                int mode = 0;
                if (speedLimiterKMH.Checked)
                {
                    mode = 1;
                }
                client.edit.speedLimiterMode = mode;
                client.edit.speedLimiterSpeed = maxSpeedBox.Text.ToDouble();
            }
        }

        private void speedLimiterMPH_CheckedChanged(object sender, EventArgs e)
        {
            int mode = 0;
            if (speedLimiterKMH.Checked)
            {
                mode = 1;
            }
            client.edit.speedLimiterMode = mode;
        }

        private void maxSpeedBox_TextChanged(object sender, EventArgs e)
        {
            client.edit.speedLimiterSpeed = maxSpeedBox.Text.ToDouble();
        }
    }
}
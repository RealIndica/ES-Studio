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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using Melanchall.DryWetMidi;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Windows.Forms.DataVisualization.Charting;
using Melanchall.DryWetMidi.Common;
using System.Net;
using System.Runtime.Remoting.Messaging;
using ClosedXML.Excel;

namespace ES_GUI
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        private string version = "0.3.0";

        private ESClient client;

        public List<Thread> threadPool;

        private Stopwatch overlayTimer;

        private Stopwatch powerBuilderTimer;
        private PowerBuilder powerBuilder;

        private bool midiLoaded = false;
        private MidiFile midiFile;
        private TempoMap tempoMap;
        private bool stopMidi = false;
        private int minMidiNote = 127;
        private int maxMidiNote = 0;

        private bool dynoLogging = false;
        private DynoUtil chartUtil;

        private bool readyToConnect = false;

        public Form1()
        {
            InitializeComponent();

            this.Text += version;

#if DEBUG
            this.Text += " - DEBUG";
#endif

            tabControl2.HandleCreated += new EventHandler(tabControl2_HandleCreated);

            positionAsset(powerBuilderRev1, powerBuilderPicture, true);
            positionAsset(powerBuilderRev2, powerBuilderPicture, true);
            positionAsset(powerBuilderGain, powerBuilderPicture, true);
            positionAsset(ledOffPicture, powerBuilderPicture, true);
            positionAsset(ledOnPicture, powerBuilderPicture, true, ledOffPicture);

            tabControl2.TabPages.RemoveAt(0);
            tabControl2.AdjustTabSizes();

            ThemeManager.ApplyTheme(this);
        }

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
            }
            else
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

        private void UpdateCheck()
        {
            #if !DEBUG
            string newVersion = string.Empty;
            try
            {
                newVersion = new WebClient().DownloadString("https://raw.githubusercontent.com/RealIndica/ES-Studio/main/version.txt");
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to check if a new version is available.\r\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!newVersion.Contains(version))
            {
                DialogResult = MessageBox.Show("A new version of ES-Studio is available.\r\nWould you like to go download it now?", "Outdated Client", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.Yes)
                {
                    Process.Start("https://github.com/RealIndica/ES-Studio/releases");
                }
            }
            #endif
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (!client.isConnected && readyToConnect)
            {
                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    #if DEBUG
                    manageControls(true);
                    manageModules(false);
                    this.Text += " - UI Preview Mode";
                    #endif
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new ESClient();

            powerBuilder = new PowerBuilder(client);

            chartUtil = new DynoUtil(dynoChart, dynoPowerLabel, dynoPowerRPMLabel, dynoTorqueLabel, dynoTorqueRPMLabel);

            threadPool = new List<Thread>();

            powerBuilderTimer = new Stopwatch();
            overlayTimer = new Stopwatch();

            quickShifterMode.Items.AddRange(new string[] { "Cut Ignition", "Retard Ignition" });
            quickShifterMode.SelectedIndex = 0;

            twoStepSwitch.Items.AddRange(new string[] { "Clutch" });
            twoStepSwitch.SelectedIndex = 0;

            twoStepLimiterModeBox.Items.AddRange(new string[] { "HARD CUT", "RETARD", "HI LO", "THROT. CUT", "FUEL CUT" });
            twoStepLimiterModeBox.SelectedIndex = 0;

            ledOffPicture.Visible = true;
            ledOnPicture.Visible = false;

            dynoChart.MouseEnter += chartUtil.dynoChart_MouseEnter;
            dynoChart.MouseLeave += chartUtil.dynoChart_MouseLeave;
            dynoChart.MouseMove += chartUtil.dynoChart_MouseMove;
            dynoChart.MouseHover += chartUtil.dynoChart_MouseHover;

            groupBox8.Parent = dynoChart;
            groupBox16.Parent = dynoChart;

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            chartUtil.ConfigureDynoChart();
            chartUtil.CreateDynoLabels();
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
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => manageControls(enabled, controls)));
                return;
            }

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
                for (int i = 1; i <= 50; i++)
                {
                    foreach (Control c in tabPage1.Controls)
                    {
                        if (c.GetType() == typeof(AquaGauge))
                        {
                            AquaGauge g = (AquaGauge)c;
                            g.Invoke((MethodInvoker)delegate
                            {
                                g.Value = (g.MaxValue / 50) * i;
                            });
                        }
                    }
                    Thread.Sleep(2);
                }
                Thread.Sleep(100);
                for (int i = 50; i >= 1; i--)
                {
                    foreach (Control c in tabPage1.Controls)
                    {
                        if (c.GetType() == typeof(AquaGauge))
                        {
                            AquaGauge g = (AquaGauge)c;
                            g.Invoke((MethodInvoker)delegate
                            {
                                g.Value = (g.MaxValue / 50) * i;
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
                UpdateCheck();
                readyToConnect = true;
            });
            t.Start();
            threadPool.Add(t);
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

                        if (client.edit.loadCalibrationMode && client.update.RPM >= client.update.maxRPM - 100)
                        {
                            client.edit.loadCalibrationMode = false;
                            client.onUpdate();
                            calibratingLabel.Invoke((MethodInvoker)delegate
                            {
                                calibratingLabel.Text = "Waiting";
                            });

                            ShowMessage("The calibration has completed", "Calibration");
                        }

                        if (!overlayTimer.IsRunning)
                        {
                            overlayTimer.Start();
                        }
                        else
                        {
                            if (overlayTimer.ElapsedMilliseconds > 50)
                            {
                                overlayTimer.Stop();
                                overlayTimer.Reset();

                                foreach (Map m in client.customMaps)
                                {
                                    if (m.enabled)
                                    {
                                        m.tableOverlay.Invalidate();
                                    }
                                }
                            }
                        }


                        if (!powerBuilderTimer.IsRunning)
                        {
                            powerBuilderTimer.Start();
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

                            if (speedoDouble.Checked)
                            {
                                speedGauge.MaxValue *= 2;
                            }

                            speedGauge.Value = (float)adjustedSpeed;
                        });

                        loadGauge.Invoke((MethodInvoker)delegate
                        {
                            loadGauge.Value = (float)client.update.engineLoad;
                            loadGauge.MaxValue = 100;
                        });

                        airGauge.Invoke((MethodInvoker) delegate
                        {
                            if (scfmSmoothing.Checked)
                            {
                                airGauge.Value = (float)client.smoothSCFM;
                            } else
                            {
                                airGauge.Value = (float)client.update.airSCFM;
                            }
                            if (air500Button.Checked) { airGauge.MaxValue = 500; return; }
                            if (air2000Button.Checked) { airGauge.MaxValue = 2000; return; }
                            if (air10000Button.Checked) { airGauge.MaxValue = 10000; return; }
                        });

                        tempGauge.Invoke((MethodInvoker)delegate
                        {
                            tempGauge.Value = (float)client.update.temperature;

                            if (tempSmoothing.Checked)
                            {
                                tempGauge.Value = (float)client.smoothTemp;
                            } else
                            {
                                tempGauge.Value = (float)client.update.temperature;
                            }

                            tempGauge.MaxValue = 4000;
                        });

                        afrGauge.Invoke((MethodInvoker)delegate
                        {
                            afrGauge.Value = (float)client.update.afr;
                            afrGauge.MaxValue = 50;
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
                                idleControlTPS.Text = Math.Round(client.update.tps, 4).ToString();
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

                        if (dynoLogging)
                        {
                            DynoLog();
                        }
                    }
                    else
                    {
                        statusLabel.Invoke((MethodInvoker)delegate
                        {
                            statusLabel.Text = "Disconnected";
                        });
                        manageControls(false);
                        client.edit.useCustomIgnitionModule = false;
                        checkBox3.Checked = false;
                        tabControl1.Invoke((MethodInvoker)delegate
                        {
                            tabControl1.SelectedIndex = 0;
                        });
                        break;
                    }

                    statusLabel.Invoke((MethodInvoker)delegate
                    {
                        statusLabel.Text = client.status;
                    });
                }
            });
            t.Start();
            threadPool.Add(t);
        }

        private void DynoLog()
        {
            if (dynoChart.InvokeRequired)
            {
                this.Invoke(new Action(DynoLog));
                return;
            }

            chartUtil.AddDataToChart(client.update.RPM, client.update.power, client.update.torque, client.update.tps * 100, client.update.sparkAdvance, client.update.airSCFM, client.update.afr);
        }

        private void ShowMessage(string message, string title)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, string>(ShowMessage), message, title);
                return;
            }

            using (CustomMessageBox msgBox = new CustomMessageBox())
            {
                Opacity = 0.9;
                msgBox.SetMessage(message, title);
                msgBox.StartPosition = FormStartPosition.CenterParent;
                msgBox.ShowDialog(this);
                Opacity = 1.0;
            }
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

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox2.Checked;
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

        private void initTwoStep()
        {
            client.edit.disableRevLimit = disableRevLimit.Checked;
            client.edit.rev1 = rev1Box.Text.ToDouble();
            client.edit.rev2 = rev2Box.Text.ToDouble();
            client.edit.rev3 = rev3Box.Text.ToDouble();
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
            client.edit.idleHelperMaxTps = idleHelperTPSMax.Text.ToDouble();
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
            }
            else
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
            }
            else
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

        private void rev3Box_TextChanged(object sender, EventArgs e)
        {
            client.edit.rev3 = rev3Box.Text.ToDouble();
        }

        private void checkIgnitionModule()
        {
            if (!checkBox3.Checked)
            {
                DialogResult result = MessageBox.Show("The custom ignition module is not enabled and your map will not work as expected until it is enabled.\r\n\r\nWould you like to enable it now?", "Custom Ignition Module Not Enabled", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    checkBox3.Checked = true;
                }
            }
        }

        private void tabControl2_MouseDown(object sender, MouseEventArgs e)
        {
            int l = tabControl2.TabCount - 1;
            if (tabControl2.GetLastTabRect().Contains(e.Location))
            {
                Map newMap = new Map(this);
                CreateMapForm f = new CreateMapForm(newMap, client.customMaps);
                f.ShowDialog(this);
                if (f.CreateForm)
                {
                    f.Dispose();
                    newMap.Create(tabControl2, client);
                    tabControl2.Selecting -= tabControl2_Selecting;
                    tabControl2.SelectedIndex = tabControl2.TabCount - 2;
                    tabControl2.Selecting += tabControl2_Selecting;
                    newMap.BuildTable();

                    tabControl2.AdjustTabSizes();

                    checkIgnitionModule();

                    return;
                }
                f.Dispose();
            }
        }

        private void tabControl2_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl2.TabCount == 1)
            {
                tabControl2.SelectedIndex = 0;
                return;
            }

            if (e.TabPageIndex == tabControl2.TabCount - 1)
            {
                e.Cancel = true;
            }
        }

        private void tabControl2_HandleCreated(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) // save
        {
            if (client.isConnected && client.customMaps.Count > 0)
            {
                List<SerializableMapData> mapDatas = new List<SerializableMapData>();
                foreach (Map m in client.customMaps)
                {
                    Debug.WriteLog("Saving Map : " + m.name);
                    mapDatas.Add(m.ToSerializableData());
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "ES-Studio Map|*.ess|All|*.*";
                saveFileDialog.Title = "Save Map";
                saveFileDialog.FileName = client.update.Name + ".ess";
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                {
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    using (GZipStream gz = new GZipStream(fs, CompressionMode.Compress))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(gz, mapDatas);
                    }
                }
                return;
            }
            MessageBox.Show("Nothing to save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e) // load
        {
            if (client.isConnected)
            {
                if (client.customMaps.Count > 0)
                {
                    DialogResult = MessageBox.Show("You already have a map open.\r\nIf you load, you will loose them.\r\nContinue?", "Active Instance", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (DialogResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "ES-Studio Map|*.ess|All|*.*";
                openFileDialog.Title = "Load Map";
                openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    List<Map> removeMaps = new List<Map>();

                    foreach (Map m in client.customMaps)
                    {
                        removeMaps.Add(m);
                    }

                    foreach (Map m in removeMaps)
                    {
                        m.Delete();
                    }

                    List<SerializableMapData> data = new List<SerializableMapData>();

                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        data = (List<SerializableMapData>)formatter.Deserialize(gz);
                    }

                    tabControl1.SelectedIndex = 1;

                    foreach (SerializableMapData d in data)
                    {
                        Debug.WriteLog("Loading Map : " + d.name);
                        Map newMap = new Map(this);
                        newMap.LoadFromSerializableData(d, tabControl2, client);
                        tabControl2.Selecting -= tabControl2_Selecting;
                        tabControl2.SelectedIndex = tabControl2.TabCount - 2;
                        tabControl2.Selecting += tabControl2_Selecting;
                    }

                    tabControl2.AdjustTabSizes();

                    checkIgnitionModule();

                    DialogResult = MessageBox.Show("Would you like to enable the map now?", "Load Map", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (DialogResult == DialogResult.Yes)
                    {
                        foreach (Map m in client.customMaps)
                        {
                            m.enable(true);
                        }
                    }
                }
            }
        }

        private void darkModeWIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //...
        }

        private void idleControlTarget_TextChanged(object sender, EventArgs e)
        {
            client.edit.idleHelperRPM = idleControlTarget.Text.ToDouble();
        }

        private void loadMidi_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Mid|*.mid|Midi|*.midi|All|*.*";
            openFileDialog.Title = "Load Midi";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                midiFile = null;
                midiFile = MidiFile.Read(openFileDialog.FileName);

                var notesAtTime = new Dictionary<TimeSpan, int>();

                foreach (var trackChunk in midiFile.GetTrackChunks())
                {
                    foreach (var noteEvent in trackChunk.GetTimedEvents().Where(ev => ev.Event is NoteOnEvent))
                    {
                        var timedEvent = (TimedEvent)noteEvent;
                        if (!notesAtTime.ContainsKey(new TimeSpan(timedEvent.Time)))
                        {
                            notesAtTime[new TimeSpan(timedEvent.Time)] = 0;
                        }
                        notesAtTime[new TimeSpan(timedEvent.Time)]++;
                    }
                }

                bool hasChords = notesAtTime.Any(kvp => kvp.Value > 1);

                if (hasChords)
                {
                    MessageBox.Show("You cannot use midi that contains chords", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    midiFile = null;
                    return;
                }
                else
                {
                    foreach (var trackChunk in midiFile.GetTrackChunks())
                    {
                        foreach (var note in trackChunk.GetNotes())
                        {
                            if (note.NoteNumber < minMidiNote) minMidiNote = note.NoteNumber;
                            if (note.NoteNumber > maxMidiNote) maxMidiNote = note.NoteNumber;
                        }
                    }

                    int range = maxMidiNote - minMidiNote;
                    Debug.WriteLog($"Minimum Note: {minMidiNote}, Maximum Note: {maxMidiNote}");
                    Debug.WriteLog($"Midi range is {range} notes");
                    maxMidiNote = minMidiNote + 12;
                    if (range > 12)
                    {
                        Debug.WriteLog("Midi not in 1 Octave range. Transposing Notes...");
                        foreach (var trackChunk in midiFile.GetTrackChunks())
                        {
                            using (var notesManager = trackChunk.ManageNotes())
                            {
                                foreach (var note in notesManager.Objects)
                                {
                                    while (note.NoteNumber < new SevenBitNumber((byte)minMidiNote))
                                    {
                                        note.NoteNumber = new SevenBitNumber((byte)(note.NoteNumber + 12));
                                    }

                                    while (note.NoteNumber > new SevenBitNumber((byte)(minMidiNote + 12)))
                                    {
                                        note.NoteNumber = new SevenBitNumber((byte)(note.NoteNumber - 12));
                                    }
                                }
                            }
                        }
                    }
                    tempoMap = midiFile.GetTempoMap();
                    midiLoaded = true;
                    midiName.Text = openFileDialog.SafeFileName;
                }
            }
        }

        private void midiThread()
        {
            Thread t = new Thread(() =>
            {
                foreach (var trackChunk in midiFile.GetTrackChunks())
                {
                    long previousNoteEndTime = 0;
                    long noteDuration = 0;

                    foreach (var note in trackChunk.GetNotes())
                    {
                        if (stopMidi)
                        {
                            stopMidi = false;
                            break;
                        }

                        int midiNoteNumber = note.NoteNumber;
                        noteDuration = note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds;
                        long noteStartTime = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds;

                        if (noteStartTime > previousNoteEndTime)
                        {
                            System.Threading.Thread.Sleep((int)((noteStartTime - previousNoteEndTime) / 1000));
                        }

                        double rpmValue = MapMidiNoteToRPM(midiNoteNumber, midiLowRPM.Text.ToDouble(), midiHighRPM.Text.ToDouble(), minMidiNote, maxMidiNote);
                        idleControlTarget.Text = rpmValue.ToString();
                        client.edit.idleHelperRPM = rpmValue;

                        Debug.WriteLog($"Note: {midiNoteNumber}, RPM: {rpmValue}");
                        previousNoteEndTime = noteStartTime + noteDuration;
                        System.Threading.Thread.Sleep((int)(noteDuration / 1000));
                    }
                    System.Threading.Thread.Sleep((int)(noteDuration / 1000));
                }

                Debug.WriteLog("Midi Ended");
                idleControlEnabled.Checked = false;
                client.edit.idleHelper = false;
                client.edit.idleHelperRPM = idleControlTarget.Text.ToDouble();
            });
            t.Start();
        }

        private double MapMidiNoteToRPM(int midiNoteNumber, double minRPM, double maxRPM, int minMidiNote, int maxMidiNote)
        {
            double normalizedNote = (double)(midiNoteNumber - minMidiNote) / (double)(maxMidiNote - minMidiNote);
            return minRPM + normalizedNote * (maxRPM - minRPM);
        }

        private void midiPlay_Click(object sender, EventArgs e)
        {
            if (midiLoaded && client.isConnected)
            {
                idleControlEnabled.Checked = true;
                client.edit.idleHelper = true;
                midiThread();

            }
        }

        private void midiPause_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To Do");
            if (midiLoaded && client.isConnected)
            {

            }
        }

        private void midiStop_Click(object sender, EventArgs e)
        {
            if (midiLoaded && client.isConnected)
            {
                idleControlEnabled.Checked = true;
                client.edit.idleHelper = false;
                stopMidi = true;
            }
        }

        private void idleHelperTPSMax_TextChanged(object sender, EventArgs e)
        {
            double originalVal = idleHelperTPSMax.Text.ToDouble();
            double val = Helpers.Clamp(idleHelperTPSMax.Text.ToDouble(), 0, 1);
            if (originalVal > 1.00 || originalVal < 0.00)
            {
                idleHelperTPSMax.Text = val.ToString() + ".00";
            }
            client.edit.idleHelperMaxTps = val;
        }

        private void midiLowRPM_TextChanged(object sender, EventArgs e)
        {
            midiHighRPM.Text = (midiLowRPM.Text.ToDouble() * 2).ToString();
        }

        private void speedoDouble_CheckedChanged(object sender, EventArgs e)
        {
            if (speedoDouble.Checked)
            {
                speedGauge.MaxValue *= 2;
                return;
            }
            speedGauge.MaxValue /= 2;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DialogResult doCalibrate = MessageBox.Show("Ensure that the engine has ran for at least 5 seconds before starting this process.\r\n\r\nMake sure engine is off.\r\nReady to calibrate?", "Load calibration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (doCalibrate == DialogResult.Yes)
            {
                client.edit.loadCalibrationMode = true;
                calibratingLabel.Text = "Calibrating";
            }
        }

        private void dynoStart_Click(object sender, EventArgs e)
        {
            dynoLogging = true;
            dynoNoDataMsg.Visible = false;
            dynoClear.ForeColor = Color.DimGray;
            dynoClear.CircleColor = Color.MidnightBlue;
        }

        private void dynoStop_Click(object sender, EventArgs e)
        {
            dynoLogging = false;
            dynoClear.ForeColor = Color.White;
            dynoClear.CircleColor = Color.MediumBlue;
        }

        private void roundButton1_Click(object sender, EventArgs e)
        {
            if (!dynoLogging)
            {
                chartUtil.ConfigureDynoChart();
                dynoNoDataMsg.Visible = true;
            }
        }

        private void smoothChartData_Click(object sender, EventArgs e)
        {
            chartUtil.SmoothAllSeries((int)smoothWindow.Value);
        }

        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThemeManager.setThemeState(true);
            ThemeManager.ApplyTheme(this);
        }

        private void lightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThemeManager.setThemeState(false);
            ShowMessage("Restart to Apply Changes", "Theme");
        }

        private void exportDataButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Document|*.xlsx|CSV (Comma Delimited)|*.csv|All|*.*";
            saveFileDialog.Title = "Export Dyno Run";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                if (saveFileDialog.FileName.ToLower().EndsWith(".xlsx"))
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("ChartData");
                        var firstSeries = dynoChart.Series[0];
                        worksheet.Cell(1, 1).Value = "RPM";
                        for (int pointIndex = 0; pointIndex < firstSeries.Points.Count; pointIndex++)
                        {
                            worksheet.Cell(pointIndex + 2, 1).Value = firstSeries.Points[pointIndex].XValue;
                        }
                        for (int i = 0; i < dynoChart.Series.Count; i++)
                        {
                            var series = dynoChart.Series[i];
                            worksheet.Cell(1, i + 2).Value = series.Name;
                            for (int pointIndex = 0; pointIndex < series.Points.Count; pointIndex++)
                            {
                                var point = series.Points[pointIndex];
                                worksheet.Cell(pointIndex + 2, i + 2).Value = point.YValues[0];
                            }
                        }
                        workbook.SaveAs(saveFileDialog.FileName);
                    }
                }
                else if (saveFileDialog.FileName.ToLower().EndsWith(".csv"))
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        var headers = new List<string> { "RPM" };
                        headers.AddRange(dynoChart.Series.Select(s => s.Name));
                        sw.WriteLine(string.Join(",", headers));

                        for (int pointIndex = 0; pointIndex < dynoChart.Series[0].Points.Count; pointIndex++)
                        {
                            var line = new List<string>{ dynoChart.Series[0].Points[pointIndex].XValue.ToString() };

                            foreach (var series in dynoChart.Series)
                            {
                                line.Add(series.Points[pointIndex].YValues[0].ToString());
                            }

                            sw.WriteLine(string.Join(",", line));
                        }
                    }
                }
            }
        }

        private void dynoShowAdvanced_CheckedChanged(object sender, EventArgs e)
        {
            chartUtil.ToggleAdvancedInfo(dynoShowAdvanced.Checked);
        }

        private void screenshotButton_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(dynoChart.Width, dynoChart.Height);
            dynoChart.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            Clipboard.SetImage(bitmap);
        }

        private void btnAdjustSelection_Click(object sender, EventArgs e)
        {
            //Get the current selected cells from the grid of the currently selected client.custommap
            var sel = client.customMaps[tabControl2.SelectedIndex].GridView.SelectedCells;

            var frmGridSelectionAdj = new FrmAdjust(ref sel);
            var dialogResult = frmGridSelectionAdj.ShowDialog();
            if ((uint)(dialogResult - 1) <= 1u)
            {
                frmGridSelectionAdj.Dispose();
            }
        }
    }
}
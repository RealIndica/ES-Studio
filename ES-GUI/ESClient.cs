using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ES_GUI
{
    public class ESClient
    {
        public bool isConnected;
        public string status;

        public double smoothRPM;
        public double smoothSCFM;
        public double smoothTemp;

        public engineUpdate update;
        public engineEdit edit;

        public List<Map> customMaps;

        private List<double> rpmSmoothingList = new List<double>();
        private double rpmSmoothingNext = 0d;
        private double rpmSmoothingLast = 0d;

        private List<double> scfmSmoothingList = new List<double>();
        private double scfmSmoothingNext = 0d;
        private double scfmSmoothingLast = 0d;

        private List<double> tempSmoothingList = new List<double>();
        private double tempSmoothingNext = 0d;
        private double tempSmoothingLast = 0d;

        private comPipe outputPipe; //send
        private NamedPipeServerStream inputPipe; //recv

        public ESClient() 
        {
            isConnected = false;
            status = "Disconnected";
            outputPipe = new comPipe("est-output-pipe");
            update = new engineUpdate();
            edit = new engineEdit();
            customMaps = new List<Map>();
        }

        public bool Connect()
        {
            string dll = Path.Combine(Environment.CurrentDirectory, "bin\\ES-CLIENT.dll");

            if (!isConnected) {

                bool alreadyInjected = Injector.IsDllLoadedInProcess("engine-sim-app", "ES-CLIENT.dll");
                bool ready = false;

                if (alreadyInjected)
                    ready = true;
                else
                    ready = Injector.InjectDLL("engine-sim-app", dll);

                if (ready)
                {
                    isConnected = true;
                    processWatcher();
                    return true;
                } 
                else
                {
                    isConnected = false;
                    return false;
                }
            }
            return true;
        }

        private void calcSmoothRPM()
        {
            double smoothResult = 0;
            if (rpmSmoothingList.Count >= 10)
            {
                foreach (double v in rpmSmoothingList)
                {
                    smoothResult += v;
                }
                smoothResult /= rpmSmoothingList.Count;
                rpmSmoothingNext = smoothResult;
                rpmSmoothingList.Clear();
            }

            if (rpmSmoothingList.Count == 0)
            {
                rpmSmoothingLast = smoothRPM;
            }

            rpmSmoothingList.Add(update.RPM);
            smoothRPM = (float)Helpers.Lerp((float)rpmSmoothingLast, (float)rpmSmoothingNext, (float)(rpmSmoothingList.Count) / 10f);
        }

        private void calcSmoothSCFM()
        {
            double smoothResult = 0;
            if (scfmSmoothingList.Count >= 10)
            {
                foreach (double v in scfmSmoothingList)
                {
                    smoothResult += v;
                }
                smoothResult /= scfmSmoothingList.Count;
                scfmSmoothingNext = smoothResult;
                scfmSmoothingList.Clear();
            }

            if (scfmSmoothingList.Count == 0)
            {
                scfmSmoothingLast = smoothSCFM;
            }

            scfmSmoothingList.Add(update.airSCFM);
            smoothSCFM = (float)Helpers.Lerp((float)scfmSmoothingLast, (float)scfmSmoothingNext, (float)(scfmSmoothingList.Count) / 10f);
        }

        private void calcSmoothTEMP()
        {
            double smoothResult = 0;
            if (tempSmoothingList.Count >= 10)
            {
                foreach (double v in tempSmoothingList)
                {
                    smoothResult += v;
                }
                smoothResult /= tempSmoothingList.Count;
                tempSmoothingNext = smoothResult;
                tempSmoothingList.Clear();
            }

            if (tempSmoothingList.Count == 0)
            {
                tempSmoothingLast = smoothTemp;
            }

            tempSmoothingList.Add(update.temperature);
            smoothTemp = (float)Helpers.Lerp((float)tempSmoothingLast, (float)tempSmoothingNext, (float)(tempSmoothingList.Count) / 10f);
        }

        private void pushEdits()
        {
            if (isConnected)
            {
                outputPipe.CommandPipe(JsonConvert.SerializeObject(edit).Replace("\n", "").Replace("\r", ""));
            }
        }

        private void processWatcher()
        {
            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                    Process[] procs = Process.GetProcessesByName("engine-sim-app");
                    if (procs.Length > 0)
                    {
                        isConnected = true;
                    }
                    else
                    {
                        isConnected = false;
                        status = "Disconnected";
                        update.Status = status;
                        break;
                    }
                }
            }).Start();
        }

        public void onUpdate()
        {
            foreach (Map m in customMaps)
            {
                m.Update();
            }         

            inputPipe = new NamedPipeServerStream("est-input-pipe", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var waitTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    inputPipe.WaitForConnection();
                } catch {  }
            }, token);

            if (!waitTask.Wait(TimeSpan.FromSeconds(0.5)))
            {
                tokenSource.Cancel();
                inputPipe.Dispose();
                return;
            }

            StreamReader reader = new StreamReader(inputPipe);
            string line = reader.ReadLine();

            try
            {
                if (Helpers.IsValidJson(line))
                {
                    update = JsonConvert.DeserializeObject<engineUpdate>(line);
                    status = update.Status;
                }
            } catch (Exception e) { Debug.WriteLog("\r\n" + e.Message); }

            inputPipe.Disconnect();
            inputPipe.Dispose();

            calcSmoothRPM();
            calcSmoothSCFM();
            calcSmoothTEMP();
            pushEdits();
        }
    }
}

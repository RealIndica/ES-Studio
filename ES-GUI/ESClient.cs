﻿using System;
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

        public engineUpdate update;
        public engineEdit edit;

        public List<Map> customMaps;

        private List<double> rpmSmoothingList = new List<double>();
        private double rpmSmoothingNext = 0d;
        private double rpmSmoothingLast = 0d;

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
                if (Injector.InjectDLL("engine-sim-app", dll))
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
            pushEdits();
        }
    }
}

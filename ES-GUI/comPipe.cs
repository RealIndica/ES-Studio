using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Windows.Forms;

namespace ES_GUI
{
    public class comPipe
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WaitNamedPipe(string name, int timeout);


        public string cmdpipename = "";

        public comPipe(string name)
        {
            this.cmdpipename = name;
        }

        public bool NamedPipeExist(string pipeName)
        {
            bool result;
            try
            {
                int timeout = 0;
                if (!WaitNamedPipe(Path.GetFullPath(string.Format("\\\\\\\\.\\\\pipe\\\\{0}", pipeName)), timeout))
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == 0)
                    {
                        result = false;
                        return result;
                    }
                    if (lastWin32Error == 2)
                    {
                        result = false;
                        return result;
                    }
                }
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public void CommandPipe(string command)
        {
            if (NamedPipeExist(cmdpipename))
            {
                new Thread(() =>
                {
                    try
                    {
                        using (NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", cmdpipename, PipeDirection.Out))
                        {
                            namedPipeClientStream.Connect();
                            using (StreamWriter streamWriter = new StreamWriter(namedPipeClientStream))
                            {
                                streamWriter.Write(command);
                                streamWriter.Dispose();
                            }
                            namedPipeClientStream.Close();
                            namedPipeClientStream.Dispose();
                        }
                    }
                    catch (IOException)
                    {
                        //no connection
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }).Start();
            }
            else
            {
                //bleh
                return;
            }
        }
    }
}

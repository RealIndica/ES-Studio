using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ES_GUI
{
    public static class Debug
    {
        public static void WriteLog<T>(T param)
        {
            #if DEBUG
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame.GetMethod();
            var className = method.DeclaringType.Name;

            string currentTime = DateTime.Now.ToString("HH:mm:ss.fff");

            Console.WriteLine($"[{currentTime}] {className}: {param}");
            #endif
        }
    }
}

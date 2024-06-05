using StardewModdingAPI;
using System.Diagnostics;

namespace MachineAugmentorsExtended
{
    internal class Log
    {
        public static IMonitor Monitor;
        public static bool IsVerbose => Monitor.IsVerbose;
        
        
        [Conditional("DEBUG")]
        public static void Debug(string str) => Log.Monitor.Log(str, LogLevel.Debug);
        
        [DebuggerHidden]
        public static void Trace(string str) => Log.Monitor.Log(str, LogLevel.Trace);

        public static void Warn(string str)
        {
            Log.Monitor.Log(str, LogLevel.Warn);
        }

        public static void Error(string str) => Log.Monitor.Log(str, LogLevel.Error);

        public static void DebugOnce(string str) => Log.Monitor.LogOnce(str, LogLevel.Debug);
    }
}
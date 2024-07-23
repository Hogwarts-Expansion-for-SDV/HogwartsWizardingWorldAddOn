using StardewModdingAPI;

namespace HogwartsAddOn
{
    internal static class Logger
    {
        private static IMonitor? Monitor { get; set; }
        private static bool DebugMode { get; set; } = false;
        internal static void Initialize(IMonitor monitor, bool debugMode = false)
        {
            Monitor = monitor;
            DebugMode = debugMode;
        }

        private static void Log(string message, LogLevel logLevel, params object[] args)
        {
            SendLog(message, logLevel, sendOnce: false, args);
        }

        internal static void LogOnce(string message, LogLevel logLevel, params object[] args)
        {
            SendLog(message, logLevel, sendOnce: true, args);
        }

        private static void SendLog(string message, LogLevel logLevel, bool sendOnce, params object[] args)
        {
            if (Monitor == null || message == null || message == string.Empty) return;
            string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            if (sendOnce) Monitor.LogOnce(formattedMessage, logLevel);
            else Monitor.Log(formattedMessage, logLevel);
        }

        internal static void Trace(string message, params object[] args)
        {
            if (DebugMode) Warn(message, args);
            else Log(message, LogLevel.Trace, args);
        }
        internal static void Debug(string message, params object[] args)
        {
            Log(message, LogLevel.Debug, args);
        }
        internal static void Info(string message, params object[] args)
        {
            Log(message, LogLevel.Info, args);
        }
        internal static void Warn(string message, params object[] args)
        {
            Log(message, LogLevel.Warn, args);
        }
        internal static void Error(string message, params object[] args)
        {
            Log(message, LogLevel.Error, args);
        }
        internal static void Verbose(string message)
        {
            if (message == null || message == string.Empty) return;
            Monitor?.VerboseLog(message);
        }
    }
}

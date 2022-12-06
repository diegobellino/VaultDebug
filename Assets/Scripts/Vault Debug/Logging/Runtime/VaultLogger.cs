using System;

namespace VaultDebug.Logging.Runtime
{
    public struct VaultLog
    {
        public LogLevel Level { get; private set; }

        public string Context { get; private set; }

        public string Message { get; private set; }

        public string TimeStamp { get; private set; }

        public string StackTrace { get; private set; }

        public VaultLog(LogLevel level, string context, string message, string stackTrace)
        {
            Level = level;
            Context = context;
            Message = message;
            StackTrace = stackTrace;

            TimeStamp = DateTime.UtcNow.ToShortTimeString();
        }
    }

    public sealed class VaultLogger
    {

        #region VARIABLES

        readonly string _context;

        #endregion

        public VaultLogger(string context)
        {
            _context = context;
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Warn(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        void Log(LogLevel level, string message)
        {
            var stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
            var log = new VaultLog(level, _context, message, stackTrace);

            VaultLogDispatcher.Instance.DispatchLog(log);
        }
    }
}

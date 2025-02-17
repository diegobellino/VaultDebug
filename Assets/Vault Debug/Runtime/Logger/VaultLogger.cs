namespace VaultDebug.Runtime.Logger
{
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
            var log = VaultLogPool.GetLog(level, _context, message, stackTrace);

            VaultLogDispatcher.DispatchLog(log);
        }
    }
}

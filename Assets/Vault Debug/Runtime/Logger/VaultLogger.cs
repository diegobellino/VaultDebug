namespace VaultDebug.Runtime.Logger
{
    public sealed class VaultLogger
    {
        readonly string _context;
        readonly IVaultLogPool _logPool;


        public VaultLogger(string context, IVaultLogPool logPool)
        {
            _context = context;
            _logPool = logPool;
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
            var log = _logPool.GetLog(level, _context, message, stackTrace);

            VaultLogDispatcher.DispatchLog(log);

            _logPool.ReleaseLog(log);
        }
    }
}

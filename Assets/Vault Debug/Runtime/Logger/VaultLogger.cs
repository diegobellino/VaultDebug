using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    public sealed class VaultLogger
    {
        readonly string _context;
        readonly IVaultLogPool _logPool;
        readonly IVaultLogDispatcher _logDispatcher;


        public VaultLogger(string context)
        {
            _context = context;

            _logPool = DIBootstrapper.Container.Resolve<IVaultLogPool>();
            _logDispatcher = DIBootstrapper.Container.Resolve<IVaultLogDispatcher>();
        }

        public void Info(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Info, message, properties);
        }

        public void Debug(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Debug, message, properties);
        }

        public void Warn(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Warn, message, properties);
        }

        public void Error(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Error, message, properties);
        }

        void Log(LogLevel level, string message, IDictionary<string, object> properties = null)
        {
            var stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
            properties ??= new Dictionary<string, object>();

            var log = _logPool.GetLog(level, _context, message, stackTrace, properties);

            _logDispatcher.DispatchLog(log);
        }
    }
}

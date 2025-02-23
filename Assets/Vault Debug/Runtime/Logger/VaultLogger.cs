using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Represents a logger that records and dispatches log messages.
    /// </summary>
    public sealed class VaultLogger
    {
        readonly string _context;
        readonly IVaultLogPool _logPool;
        readonly IVaultLogDispatcher _logDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultLogger"/> class for the specified context.
        /// </summary>
        /// <param name="context">The context for the logger.</param>
        public VaultLogger(string context)
        {
            _context = context;
            _logPool = DIBootstrapper.Container.Resolve<IVaultLogPool>();
            _logDispatcher = DIBootstrapper.Container.Resolve<IVaultLogDispatcher>();
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The informational message.</param>
        /// <param name="properties">Optional additional properties.</param>
        public void Info(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Info, message, properties);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="properties">Optional additional properties.</param>
        public void Debug(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Debug, message, properties);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="properties">Optional additional properties.</param>
        public void Warn(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Warn, message, properties);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="properties">Optional additional properties.</param>
        public void Error(string message, IDictionary<string, object> properties = null)
        {
            Log(LogLevel.Error, message, properties);
        }

        /// <summary>
        /// Logs a message with the specified log level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="properties">Optional additional properties.</param>
        void Log(LogLevel level, string message, IDictionary<string, object> properties = null)
        {
            var stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
            properties ??= new Dictionary<string, object>();

            var log = _logPool.GetLog(level, _context, message, stackTrace, properties);
            _logDispatcher.DispatchLog(log);
            _logPool.ReleaseLog(log);
        }
    }
}

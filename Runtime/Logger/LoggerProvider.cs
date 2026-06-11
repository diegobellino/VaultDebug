using System.Collections.Generic;
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Provides and manages logger instances for various contexts.
    /// </summary>
    public class LoggerProvider : ILoggerProvider
    {
        private readonly Dictionary<string, VaultLogger> _loggers = new();

        /// <summary>
        /// Gets a logger for the specified context. If a logger does not exist for the context, a new one is created.
        /// </summary>
        /// <param name="context">The context for which to get the logger.</param>
        /// <returns>A <see cref="VaultLogger"/> instance for the specified context.</returns>
        public VaultLogger GetLogger(string context, Color? color = null)
        {
            if (_loggers.ContainsKey(context))
            {
                return _loggers[context];
            }

            var newLogger = new VaultLogger(context, color);
            _loggers.Add(context, newLogger);
            return newLogger;
        }
    }
}

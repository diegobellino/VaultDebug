using System;
using System.Collections.Generic;

namespace VaultDebug.Logging.Runtime
{
    [Flags()]
    public enum LogLevel
    {
        Info = 2,
        Debug = 4,
        Warn = 8,
        Error = 16,
        Exception = 32
    }

    public sealed class VaultLoggerFactory
    {

        #region VARIABLES

        static Dictionary<string, VaultLogger> _loggers = new();

        #endregion

        public static VaultLogger GetOrCreateLogger(string context)
        {
            if (_loggers.ContainsKey(context))
            {
                return _loggers[context];
            }         
            
            var newLogger = new VaultLogger(context);
            _loggers[context] = newLogger;

            return newLogger;
        }
    }
}

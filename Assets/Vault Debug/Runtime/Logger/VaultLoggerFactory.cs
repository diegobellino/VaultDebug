using System;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Debug = 2,
        Warn = 4,
        Error = 8,
        Exception = 16
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

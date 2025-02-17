using System;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Info = 1, // 1
        Debug = 2, // 10
        Warn = 4, // 100
        Error = 8, // 1000
        Exception = 16 // 10000
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

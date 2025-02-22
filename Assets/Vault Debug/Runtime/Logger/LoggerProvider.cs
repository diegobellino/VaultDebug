using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    public class LoggerProvider : ILoggerProvider
    {
        private readonly Dictionary<string, VaultLogger> _loggers = new();

        public VaultLogger GetLogger(string context)
        {
            if (_loggers.ContainsKey(context))
            {
                return _loggers[context];
            }

            var newLogger = new VaultLogger(context);
            _loggers.Add(context, newLogger);
            return newLogger;
        }
    }
}

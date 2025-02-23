using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    public class VaultLog: IVaultLog
    {
        public long Id { get; private set; }

        public LogLevel Level { get; private set; }

        public string Context { get; private set; }

        public string Message { get; private set; }

        public long TimeStampTicks { get; private set; }

        public string Stacktrace { get; private set; }

        public IDictionary<string, object> Properties { get; private set; }

        ILogIdProvider _logIdProvider;

        public VaultLog(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null, long id = -1)
        {
            _logIdProvider = DIBootstrapper.Container.Resolve<ILogIdProvider>();

            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = id == -1 ? _logIdProvider.GetNextId() : id;
            Properties = properties ?? new Dictionary<string, object>();

            TimeStampTicks = DateTime.Now.Ticks;
        }

        public void Init(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null)
        {
            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = _logIdProvider.GetNextId();
            Properties = properties ?? new Dictionary<string, object>();

            TimeStampTicks = DateTime.Now.Ticks;
        }

        public int CompareTo(IVaultLog other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}

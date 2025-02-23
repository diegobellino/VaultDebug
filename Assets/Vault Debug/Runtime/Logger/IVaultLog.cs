using System;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    public interface IVaultLog : IComparable<IVaultLog>
    {
        public long Id { get; }

        public LogLevel Level { get; }

        public string Context { get; }

        public string Message { get; }

        public long TimeStampTicks { get; }

        public string Stacktrace { get; }

        public IDictionary<string, object> Properties { get; }

        void Init(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null);
    }
}

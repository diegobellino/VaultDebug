using System;

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

        void Init(LogLevel level, string context, string message, string stackTrace);
    }
}

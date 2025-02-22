using System;

namespace VaultDebug.Runtime.Logger
{
    public class VaultLog: IComparable<VaultLog>
    {
        private static int _nextLogId = 0;

        public long Id { get; private set; }

        public LogLevel Level { get; private set; }

        public string Context { get; private set; }

        public string Message { get; private set; }

        public long TimeStampTicks { get; private set; }

        public string Stacktrace { get; private set; }

        public VaultLog(LogLevel level, string context, string message, string stackTrace)
        {
            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = GetNextId();

            TimeStampTicks = DateTime.Now.Ticks;
        }

        public void Init(LogLevel level, string context, string message, string stackTrace)
        {
            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = GetNextId();
            TimeStampTicks = DateTime.Now.Ticks;
        }

        private static long GetNextId()
        {
            return System.Threading.Interlocked.Increment(ref _nextLogId);
        }

        public int CompareTo(VaultLog other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}

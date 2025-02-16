using System;

namespace VaultDebug.Logging.Runtime
{
    public struct VaultLog: IComparable<VaultLog>
    {
        private static int _nextLogId = 0;

        public int Id { get; private set; }

        public LogLevel Level { get; private set; }

        public string Context { get; private set; }

        public string Message { get; private set; }

        public string TimeStamp { get; private set; }

        public string Stacktrace { get; private set; }

        public VaultLog(LogLevel level, string context, string message, string stackTrace)
        {
            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = GetNextId();

            TimeStamp = DateTime.Now.ToLongTimeString();
        }

        public void Init(LogLevel level, string context, string message, string stackTrace)
        {
            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = GetNextId();
            TimeStamp = DateTime.Now.ToLongTimeString();
        }

        private static int GetNextId()
        {
            return System.Threading.Interlocked.Increment(ref _nextLogId);
        }

        public int CompareTo(VaultLog other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}

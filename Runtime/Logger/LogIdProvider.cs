namespace VaultDebug.Runtime.Logger
{
    public class LogIdProvider : ILogIdProvider
    {
        long _nextId;

        public long GetNextId()
        {
            return System.Threading.Interlocked.Increment(ref _nextId);
        }

        public void Reset()
        {
           System.Threading.Interlocked.Exchange(ref _nextId, 0);
        }
    }
}

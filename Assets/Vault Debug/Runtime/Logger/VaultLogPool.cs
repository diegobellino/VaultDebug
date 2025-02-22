using System.Collections.Concurrent;

namespace VaultDebug.Runtime.Logger
{
    public class VaultLogPool: IVaultLogPool
    {
        private readonly ConcurrentQueue<IVaultLog> _pool = new();

        public IVaultLog GetLog(LogLevel level, string context, string message, string stackTrace)
        {
            if (_pool.Count > 0)
            {
                if (_pool.TryDequeue(out var log))
                {
                    log.Init(level, context, message, stackTrace);
                    return log;
                }
            }

            return new VaultLog(level, context, message, stackTrace); ;
        }

        public void ReleaseLog(IVaultLog log)
        {
            _pool.Enqueue(log);
        }
    }
}
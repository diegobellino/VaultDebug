using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    public class VaultLogPool: IVaultLogPool
    {
        private readonly ConcurrentQueue<IVaultLog> _pool = new();

        public IVaultLog GetLog(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null)
        {
            if (_pool.Count > 0)
            {
                if (_pool.TryDequeue(out var log))
                {
                    log.Init(level, context, message, stackTrace, properties);
                    return log;
                }
            }

            return new VaultLog(level, context, message, stackTrace, properties);
        }

        public void ReleaseLog(IVaultLog log)
        {
            _pool.Enqueue(log);
        }
    }
}
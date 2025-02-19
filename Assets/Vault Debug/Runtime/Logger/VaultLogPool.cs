using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    public static class VaultLogPool
    {
        private static readonly Queue<VaultLog> _pool = new();

        public static VaultLog GetLog(LogLevel level, string context, string message, string stackTrace)
        {
            if (_pool.Count > 0)
            {
                var log = _pool.Dequeue();
                log.Init(level, context, message, stackTrace);
                return log;
            }

            return new VaultLog(level, context, message, stackTrace); ;
        }

        public static void ReleaseLog(VaultLog log)
        {
            _pool.Enqueue(log);
        }
    }
}
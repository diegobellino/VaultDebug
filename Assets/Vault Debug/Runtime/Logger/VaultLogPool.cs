using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Implements a pool to manage <see cref="IVaultLog"/> instances for reuse.
    /// </summary>
    public class VaultLogPool : IVaultLogPool
    {
        private readonly ConcurrentQueue<IVaultLog> _pool = new();

        /// <summary>
        /// Retrieves a log instance from the pool if available; otherwise, creates a new log.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="context">The context associated with the log.</param>
        /// <param name="message">The log message.</param>
        /// <param name="stackTrace">The stack trace at the log point.</param>
        /// <param name="properties">Optional additional properties.</param>
        /// <returns>An instance of <see cref="IVaultLog"/>.</returns>
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

        /// <summary>
        /// Releases a log instance back into the pool for reuse.
        /// </summary>
        /// <param name="log">The log instance to release.</param>
        public void ReleaseLog(IVaultLog log)
        {
            _pool.Enqueue(log);
        }
    }
}

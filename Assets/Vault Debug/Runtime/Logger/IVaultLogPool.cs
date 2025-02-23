using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Represents a pool for reusing <see cref="IVaultLog"/> instances.
    /// </summary>
    public interface IVaultLogPool
    {
        /// <summary>
        /// Retrieves a log instance from the pool or creates a new one if none are available.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="context">The log context.</param>
        /// <param name="message">The log message.</param>
        /// <param name="stackTrace">The stack trace for the log.</param>
        /// <param name="properties">Optional additional properties.</param>
        /// <returns>An instance of <see cref="IVaultLog"/>.</returns>
        IVaultLog GetLog(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null);

        /// <summary>
        /// Releases a log instance back into the pool for reuse.
        /// </summary>
        /// <param name="log">The log to release.</param>
        void ReleaseLog(IVaultLog log);
    }
}

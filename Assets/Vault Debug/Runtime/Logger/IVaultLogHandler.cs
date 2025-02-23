namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Defines methods for handling logs and managing log listeners.
    /// </summary>
    public interface IVaultLogHandler
    {
        /// <summary>
        /// Gets the maximum number of logs that can be cached.
        /// </summary>
        int MaxLogCached { get; }

        /// <summary>
        /// Registers a log listener.
        /// </summary>
        /// <param name="listener">The log listener to register.</param>
        void RegisterLogListener(IVaultLogListener listener);

        /// <summary>
        /// Unregisters a log listener.
        /// </summary>
        /// <param name="listener">The log listener to unregister.</param>
        void UnregisterLogListener(IVaultLogListener listener);

        /// <summary>
        /// Handles an incoming log.
        /// </summary>
        /// <param name="log">The log to handle.</param>
        void HandleLog(IVaultLog log);
    }
}

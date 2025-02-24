namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Defines methods for dispatching logs to registered handlers.
    /// </summary>
    public interface IVaultLogDispatcher
    {
        /// <summary>
        /// Registers a log handler.
        /// </summary>
        /// <param name="handler">The log handler to register.</param>
        /// <param name="listeningContexts">Optional contexts the handler is interested in.</param>
        void RegisterHandler(IVaultLogHandler handler, string[] listeningContexts = null);

        /// <summary>
        /// Unregisters a previously registered log handler.
        /// </summary>
        /// <param name="handler">The log handler to unregister.</param>
        void UnregisterHandler(IVaultLogHandler handler);

        /// <summary>
        /// Dispatches a log to the appropriate registered handlers.
        /// </summary>
        /// <param name="log">The log to dispatch.</param>
        void DispatchLog(IVaultLog log);
    }
}

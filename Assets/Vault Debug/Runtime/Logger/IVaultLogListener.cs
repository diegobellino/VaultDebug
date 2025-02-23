namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Defines a listener for refreshing logs.
    /// </summary>
    public interface IVaultLogListener
    {
        /// <summary>
        /// Refreshes the logs.
        /// </summary>
        void RefreshLogs();
    }
}

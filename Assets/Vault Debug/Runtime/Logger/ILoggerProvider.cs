namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Provides logger instances for specific contexts.
    /// </summary>
    public interface ILoggerProvider
    {
        /// <summary>
        /// Gets a logger for the specified context.
        /// </summary>
        /// <param name="context">The context for which to get the logger.</param>
        /// <returns>A <see cref="VaultLogger"/> instance associated with the context.</returns>
        VaultLogger GetLogger(string context);
    }
}

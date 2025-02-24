namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Provides methods for generating and resetting unique log IDs.
    /// </summary>
    public interface ILogIdProvider
    {
        /// <summary>
        /// Gets the next unique log ID.
        /// </summary>
        /// <returns>The next unique ID as a long integer.</returns>
        long GetNextId();

        /// <summary>
        /// Resets the log ID counter.
        /// </summary>
        void Reset();
    }
}

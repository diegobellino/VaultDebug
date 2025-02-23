using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Provides methods for saving, loading, and exporting log data.
    /// </summary>
    public interface ILogStorageService
    {
        /// <summary>
        /// Asynchronously saves a collection of logs.
        /// </summary>
        /// <param name="logs">The collection of logs to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveLogsAsync(IEnumerable<IVaultLog> logs);

        /// <summary>
        /// Asynchronously loads logs.
        /// </summary>
        /// <returns>A task that returns a collection of logs.</returns>
        Task<IEnumerable<IVaultLog>> LoadLogsAsync();

        /// <summary>
        /// Asynchronously exports logs to the specified path.
        /// </summary>
        /// <param name="logs">The collection of logs to export.</param>
        /// <param name="exportPath">The path to which logs are exported.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExportLogsAsync(IEnumerable<IVaultLog> logs, string exportPath);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaultDebug.Runtime.Logger
{
    public interface ILogStorageService
    {
        Task SaveLogsAsync(IEnumerable<IVaultLog> logs);
        Task<IEnumerable<IVaultLog>> LoadLogsAsync();
        Task ExportLogsAsync(IEnumerable<IVaultLog> logs, string exportPath);
    }
}

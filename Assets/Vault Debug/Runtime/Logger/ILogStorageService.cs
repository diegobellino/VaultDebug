using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaultDebug.Runtime.Logger
{
    public interface ILogStorageService
    {
        Task SaveLogsAsync(IEnumerable<VaultLog> logs);
        Task<IEnumerable<VaultLog>> LoadLogsAsync();
        Task ExportLogsAsync(IEnumerable<VaultLog> logs, string exportPath);
    }
}

using System.Collections.Generic;

namespace Vault.Logging.Runtime
{
    public interface IVaultLogListener
    {
        void RefreshLogs();
    }
}
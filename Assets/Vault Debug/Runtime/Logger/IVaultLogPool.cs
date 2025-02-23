
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    public interface IVaultLogPool
    {
        IVaultLog GetLog(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null);
        void ReleaseLog(IVaultLog log);
    }
}

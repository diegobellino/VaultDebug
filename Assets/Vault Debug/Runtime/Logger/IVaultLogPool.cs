
namespace VaultDebug.Runtime.Logger
{
    public interface IVaultLogPool
    {
        IVaultLog GetLog(LogLevel level, string context, string message, string stackTrace);
        void ReleaseLog(IVaultLog log);
    }
}

namespace VaultDebug.Logging.Runtime
{
    public interface IVaultLogHandler
    {
        void RegisterListener(IVaultLogListener listener);
        void UnregisterListener(IVaultLogListener listener);

        void HandleVaultLog(VaultLog log);
    }
}
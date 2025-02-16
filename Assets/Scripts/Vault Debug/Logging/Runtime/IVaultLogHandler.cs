namespace VaultDebug.Logging.Runtime
{
    public interface IVaultLogHandler
    {
        void RegisterLogListener(IVaultLogListener listener);
        void UnregisterLogListener(IVaultLogListener listener);

        void HandleLog(VaultLog log);
    }
}
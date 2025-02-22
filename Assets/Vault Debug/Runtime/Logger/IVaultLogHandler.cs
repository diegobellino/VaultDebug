namespace VaultDebug.Runtime.Logger
{
    public interface IVaultLogHandler
    {
        int MaxLogCached { get; }

        void RegisterLogListener(IVaultLogListener listener);
        void UnregisterLogListener(IVaultLogListener listener);

        void HandleLog(VaultLog log);
    }
}
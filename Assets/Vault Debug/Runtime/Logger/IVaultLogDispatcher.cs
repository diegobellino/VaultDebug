namespace VaultDebug.Runtime.Logger
{
    public interface IVaultLogDispatcher
    {
        void RegisterHandler(IVaultLogHandler handler, string[] listeningContexts = null);
        void UnregisterHandler(IVaultLogHandler handler);
        void DispatchLog(IVaultLog log);
    }
}

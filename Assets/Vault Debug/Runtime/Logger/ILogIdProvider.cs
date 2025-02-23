namespace VaultDebug.Runtime.Logger
{
    public interface ILogIdProvider
    {
        long GetNextId();
        void Reset();
    }
}

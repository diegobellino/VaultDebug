namespace VaultDebug.Runtime.Logger
{
    public interface ILoggerProvider
    {
        VaultLogger GetLogger(string context);
    }
}

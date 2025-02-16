using VaultDebug.Logging.Runtime;

namespace VaultDebug.Console.Editor
{
    internal static class VaultDebugLoggerInternal
    {
        public static VaultLogger Logger = VaultLoggerFactory.GetOrCreateLogger("VaultDebugInternal");
    }
}

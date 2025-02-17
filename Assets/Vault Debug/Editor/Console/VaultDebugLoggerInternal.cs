using VaultDebug.Runtime.Logger;

namespace VaultDebug.Editor.Console
{
    internal static class VaultDebugLoggerInternal
    {
        public static VaultLogger Logger = VaultLoggerFactory.GetOrCreateLogger("VaultDebugInternal");
    }
}

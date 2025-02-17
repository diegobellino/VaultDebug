using VaultDebug.Runtime.Logger;

namespace VaultDebug.Editor.Console
{
    public static class VaultDebugLoggerSample
    {

        public static VaultLogger Logger = VaultLoggerFactory.GetOrCreateLogger("VaultDebugSample");
    }
}

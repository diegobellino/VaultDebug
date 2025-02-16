
using VaultDebug.Logging.Runtime;

namespace VaultDebug.Console.Editor
{
    public static class VaultDebugLoggerSample
    {

        public static VaultLogger Logger = VaultLoggerFactory.GetOrCreateLogger("VaultDebugSample");
    }
}

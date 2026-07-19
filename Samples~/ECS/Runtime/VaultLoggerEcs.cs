using UnityEngine;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Runtime.Logger.Entities
{
    /// <summary>
    /// Creates Burst-safe logger values during ECS system setup.
    /// </summary>
    public static class VaultLoggerEcs
    {
        /// <summary>
        /// Create this value from an <c>ISystem.OnCreate</c> method, then copy it into jobs.
        /// </summary>
        public static VaultLogger CreateLogger(string context, Color? color = null, int maxProperties = VaultLogProperties.Capacity)
        {
            return DIBootstrapper.Container
                .Resolve<ILoggerProvider>()
                .GetLogger(context, color, maxProperties);
        }
    }
}

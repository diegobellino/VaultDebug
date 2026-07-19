using UnityEngine;
using System;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Provides logger instances for specific contexts.
    /// </summary>
    public interface ILoggerProvider : IDisposable
    {
        /// <summary>
        /// Gets a logger for the specified context.
        /// </summary>
        /// <param name="context">The context for which to get the logger.</param>
        /// <param name="color">Optional color for the context tag.</param>
        /// <param name="maxProperties">Maximum structured properties retained per record, from zero through <see cref="VaultLogProperties.Capacity"/>.</param>
        /// <returns>A <see cref="VaultLogger"/> instance associated with the context.</returns>
        VaultLogger GetLogger(string context, Color? color = null, int maxProperties = VaultLogProperties.Capacity);

        /// <summary>Forwards all currently queued records to managed handlers.</summary>
        void Drain();
    }
}

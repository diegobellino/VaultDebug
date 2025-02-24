using System.Collections.Generic;
using System.Linq;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Dispatches logs to registered log handlers.
    /// </summary>
    public class VaultLogDispatcher : IVaultLogDispatcher
    {
        private readonly Dictionary<IVaultLogHandler, string[]> _handlers = new();

        /// <summary>
        /// Registers a log handler with optional listening contexts.
        /// </summary>
        /// <param name="handler">The log handler to register.</param>
        /// <param name="listeningContexts">Optional contexts that the handler listens to.</param>
        public void RegisterHandler(IVaultLogHandler handler, string[] listeningContexts = null)
        {
            if (!_handlers.ContainsKey(handler))
            {
                _handlers.Add(handler, listeningContexts);
            }
        }

        /// <summary>
        /// Unregisters a log handler.
        /// </summary>
        /// <param name="handler">The log handler to unregister.</param>
        public void UnregisterHandler(IVaultLogHandler handler)
        {
            if (_handlers.ContainsKey(handler))
            {
                _handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Dispatches a log to all handlers that are registered for its context.
        /// </summary>
        /// <param name="log">The log to dispatch.</param>
        public void DispatchLog(IVaultLog log)
        {
            foreach (var handlerKeyValue in _handlers)
            {
                var handler = handlerKeyValue.Key;
                var contexts = handlerKeyValue.Value;

                if (contexts == null || contexts.Any(item => item.Equals(log.Context)))
                {
                    handler.HandleLog(log);
                }
            }
        }
    }
}

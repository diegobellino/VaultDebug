using System.Collections.Generic;
using System.Linq;

namespace Vault.Logging.Runtime
{
    public sealed class VaultLogDispatcher
    {

        #region VARIABLES

        readonly static VaultLogDispatcher _instance = new();

        /// <summary>
        /// Retrieves the unique static instance of the log dispatcher
        /// </summary>
        public static VaultLogDispatcher Instance => _instance;

        Dictionary<IVaultLogHandler, string[]> _handlers = new();

        #endregion

        VaultLogDispatcher()
        {

        }

        public void RegisterHandler(IVaultLogHandler handler, string[] forContexts = null)
        {
            if (!_handlers.ContainsKey(handler))
            {
                _handlers.Add(handler, forContexts);
            }
        }

        public void UnregisterHandler(IVaultLogHandler handler)
        {
            if (_handlers.ContainsKey(handler))
            {
                _handlers.Remove(handler);
            }
        }

        public void DispatchLog(VaultLog log)
        {
            foreach(var handlerKeyValue in _handlers)
            {
                var handler = handlerKeyValue.Key;
                var contexts = handlerKeyValue.Value;
                
                if (contexts == null || contexts.Any(item => item.Equals(log.Context)))
                {
                    handler.HandleVaultLog(log);
                }
            }
        }
    }
}
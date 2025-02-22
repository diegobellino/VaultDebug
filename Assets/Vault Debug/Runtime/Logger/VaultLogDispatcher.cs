using System.Collections.Generic;
using System.Linq;

namespace VaultDebug.Runtime.Logger
{
    public class VaultLogDispatcher: IVaultLogDispatcher
    {
        Dictionary<IVaultLogHandler, string[]> _handlers = new();

        public void RegisterHandler(IVaultLogHandler handler, string[] listeningContexts = null)
        {
            if (!_handlers.ContainsKey(handler))
            {
                _handlers.Add(handler, listeningContexts);
            }
        }

        public void UnregisterHandler(IVaultLogHandler handler)
        {
            if (_handlers.ContainsKey(handler))
            {
                _handlers.Remove(handler);
            }
        }

        public void DispatchLog(IVaultLog log)
        {
            foreach(var handlerKeyValue in _handlers)
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
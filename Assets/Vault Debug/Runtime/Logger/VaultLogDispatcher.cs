using System.Collections.Generic;
using System.Linq;

namespace VaultDebug.Runtime.Logger
{
    public static class VaultLogDispatcher
    {

        #region VARIABLES

        static Dictionary<IVaultLogHandler, string[]> _handlers = new();

        #endregion

        static VaultLogDispatcher()
        {

        }

        public static void RegisterHandler(IVaultLogHandler handler, string[] listeningContexts = null)
        {
            if (!_handlers.ContainsKey(handler))
            {
                _handlers.Add(handler, listeningContexts);
            }
        }

        public static void UnregisterHandler(IVaultLogHandler handler)
        {
            if (_handlers.ContainsKey(handler))
            {
                _handlers.Remove(handler);
            }
        }

        public static void DispatchLog(IVaultLog log)
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
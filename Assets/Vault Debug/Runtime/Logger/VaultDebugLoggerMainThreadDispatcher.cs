using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    public class VaultDebugLoggerMainThreadDispatcher: MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new();

        private static VaultDebugLoggerMainThreadDispatcher _instance;

        public static VaultDebugLoggerMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                var obj = new GameObject("VaultDebugLoggerMainThreadDispatcher");
                _instance = obj.AddComponent<VaultDebugLoggerMainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }

            return _instance;
        }

        public void Enqueue(Action action)
        {
             _executionQueue.Enqueue(action);
        }

        private void Update()
        {
            while (_executionQueue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }
}

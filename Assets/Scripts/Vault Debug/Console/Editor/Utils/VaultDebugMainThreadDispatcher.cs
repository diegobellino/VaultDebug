using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace VaultDebug.Console.Editor.Utils
{
    class VaultDebugMainThreadDispatcher: MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new();

        private static VaultDebugMainThreadDispatcher _instance;

        public static VaultDebugMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                var obj = new GameObject("VaultDebugMainThreadDispatcher");
                _instance = obj.AddComponent<VaultDebugMainThreadDispatcher>();
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

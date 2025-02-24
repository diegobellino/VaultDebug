using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// A MonoBehaviour that dispatches actions on the main thread.
    /// </summary>
    public class VaultDebugLoggerMainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new();
        private static VaultDebugLoggerMainThreadDispatcher _instance;
        private static Action<GameObject> _dontDestroyOnLoad = obj => DontDestroyOnLoad(obj);

        /// <summary>
        /// Gets the main thread dispatcher instance, creating it if necessary.
        /// </summary>
        /// <param name="dontDestroyOverride">Optional override for the DontDestroyOnLoad behavior (used for testing).</param>
        /// <returns>The <see cref="VaultDebugLoggerMainThreadDispatcher"/> instance.</returns>
        public static VaultDebugLoggerMainThreadDispatcher Instance(Action<GameObject> dontDestroyOverride = null)
        {
            if (_instance == null)
            {
                var obj = new GameObject("VaultDebugLoggerMainThreadDispatcher");
                _instance = obj.AddComponent<VaultDebugLoggerMainThreadDispatcher>();

                if (dontDestroyOverride == null)
                    _dontDestroyOnLoad(obj);
                else
                    dontDestroyOverride(obj); // Mocked for testing
            }
            return _instance;
        }

        /// <summary>
        /// Enqueues an action to be executed on the main thread.
        /// </summary>
        /// <param name="action">The action to enqueue.</param>
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

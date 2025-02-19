using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    public class VaultDebugLoggerMainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> _executionQueue = new();
    private static VaultDebugLoggerMainThreadDispatcher _instance;
    private static Action<GameObject> _dontDestroyOnLoad = obj => DontDestroyOnLoad(obj);

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

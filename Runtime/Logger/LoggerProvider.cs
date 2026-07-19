using System;
using Unity.Collections;
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Owns the native log queue and creates Burst-safe <see cref="VaultLogger"/> values.
    /// </summary>
    public class LoggerProvider : ILoggerProvider
    {
        readonly NativeQueue<VaultLogRecord> _queue;
        readonly IVaultLogPool _logPool;
        readonly IVaultLogDispatcher _logDispatcher;
        bool _isSubscribed;
        bool _isDisposed;

        public LoggerProvider(IVaultLogPool logPool, IVaultLogDispatcher logDispatcher)
        {
            _queue = new NativeQueue<VaultLogRecord>(Allocator.Persistent);
            _logPool = logPool;
            _logDispatcher = logDispatcher;
            Application.quitting += Dispose;
        }

        /// <summary>
        /// Gets a value-type logger for the specified context.
        /// </summary>
        /// <param name="context">The context for which to get the logger.</param>
        /// <returns>A <see cref="VaultLogger"/> instance for the specified context.</returns>
        public VaultLogger GetLogger(string context, Color? color = null, int maxProperties = VaultLogProperties.Capacity)
        {
            ThrowIfDisposed();
            EnsureMainThreadDrain();

            var color32 = color.HasValue ? (Color32) color.Value : default;
            return new VaultLogger(
                _queue.AsParallelWriter(),
                new FixedString64Bytes(context ?? string.Empty),
                color.HasValue ? (byte)1 : (byte)0,
                color32.r,
                color32.g,
                color32.b,
                color32.a,
                (byte)maxProperties);
        }

        /// <summary>Forwards all queued records to the managed VaultDebug handlers.</summary>
        public void Drain()
        {
            ThrowIfDisposed();
            while (_queue.TryDequeue(out var record))
            {
                Color? color = record.HasColor != 0
                    ? (Color?)(Color)new Color32(record.ColorR, record.ColorG, record.ColorB, record.ColorA)
                    : null;
                var log = _logPool.GetLog(
                    record.Level,
                    record.Context.ToString(),
                    record.Message.ToString(),
                    string.Empty,
                    record.Properties.ToManagedDictionary(),
                    color);

                try
                {
                    _logDispatcher.DispatchLog(log);
                }
                finally
                {
                    _logPool.ReleaseLog(log);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_isSubscribed)
            {
                VaultDebugLoggerMainThreadDispatcher.MainThreadUpdate -= Drain;
                VaultDebugLoggerMainThreadDispatcher.MainThreadShutdown -= Dispose;
                _isSubscribed = false;
            }

            if (_queue.IsCreated)
            {
                _queue.Dispose();
            }

            Application.quitting -= Dispose;
            _isDisposed = true;
        }

        void EnsureMainThreadDrain()
        {
            if (_isSubscribed)
            {
                return;
            }

            VaultDebugLoggerMainThreadDispatcher.Instance();
            VaultDebugLoggerMainThreadDispatcher.MainThreadUpdate += Drain;
            VaultDebugLoggerMainThreadDispatcher.MainThreadShutdown += Dispose;
            _isSubscribed = true;
        }

        void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(LoggerProvider));
            }
        }
    }
}

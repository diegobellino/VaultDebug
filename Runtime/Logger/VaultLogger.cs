using Unity.Collections;
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// An unmanaged logger that can be used by ordinary Unity code and Burst-compiled jobs.
    /// </summary>
    public struct VaultLogger
    {
        readonly NativeQueue<VaultLogRecord>.ParallelWriter _writer;
        readonly FixedString64Bytes _context;
        readonly byte _hasColor;
        readonly byte _colorR;
        readonly byte _colorG;
        readonly byte _colorB;
        readonly byte _colorA;
        readonly byte _maxProperties;

        /// <summary>
        /// Initializes a new value with its native queue writer and context metadata.
        /// </summary>
        internal VaultLogger(
            NativeQueue<VaultLogRecord>.ParallelWriter writer,
            FixedString64Bytes context,
            byte hasColor,
            byte colorR,
            byte colorG,
            byte colorB,
            byte colorA,
            byte maxProperties)
        {
            _context = context;
            _writer = writer;
            _hasColor = hasColor;
            _colorR = colorR;
            _colorG = colorG;
            _colorB = colorB;
            _colorA = colorA;
            _maxProperties = maxProperties;
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public void Info(FixedString512Bytes message, VaultLogProperties properties = default)
        {
            Log(LogLevel.Info, message, properties);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        public void Debug(FixedString512Bytes message, VaultLogProperties properties = default)
        {
            Log(LogLevel.Debug, message, properties);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public void Warn(FixedString512Bytes message, VaultLogProperties properties = default)
        {
            Log(LogLevel.Warn, message, properties);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public void Error(FixedString512Bytes message, VaultLogProperties properties = default)
        {
            Log(LogLevel.Error, message, properties);
        }

        /// <summary>
        /// Queues an informational message from managed OOP code.
        /// </summary>
        public void Info(string message) => Info(new FixedString512Bytes(message ?? string.Empty));

        /// <summary>Queues a debug message from managed OOP code.</summary>
        public void Debug(string message) => Debug(new FixedString512Bytes(message ?? string.Empty));

        /// <summary>Queues a warning message from managed OOP code.</summary>
        public void Warn(string message) => Warn(new FixedString512Bytes(message ?? string.Empty));

        /// <summary>Queues an error message from managed OOP code.</summary>
        public void Error(string message) => Error(new FixedString512Bytes(message ?? string.Empty));

        void Log(LogLevel level, FixedString512Bytes message, VaultLogProperties properties)
        {
            properties.Truncate(_maxProperties);
            _writer.Enqueue(new VaultLogRecord
            {
                Level = level,
                Context = _context,
                Message = message,
                Properties = properties,
                HasColor = _hasColor,
                ColorR = _colorR,
                ColorG = _colorG,
                ColorB = _colorB,
                ColorA = _colorA
            });
        }
    }
}

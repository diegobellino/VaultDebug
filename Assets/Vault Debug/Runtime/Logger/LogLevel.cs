using System;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Specifies the severity levels for logging.
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// No logging.
        /// </summary>
        None = 0,
        /// <summary>
        /// Informational logging.
        /// </summary>
        Info = 1, // 1
        /// <summary>
        /// Debug-level logging.
        /// </summary>
        Debug = 2, // 10
        /// <summary>
        /// Warning logging.
        /// </summary>
        Warn = 4, // 100
        /// <summary>
        /// Error logging.
        /// </summary>
        Error = 8, // 1000
        /// <summary>
        /// Exception logging.
        /// </summary>
        Exception = 16 // 10000
    }
}

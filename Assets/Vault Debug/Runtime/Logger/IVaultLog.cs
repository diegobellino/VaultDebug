using System;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Represents a log entry with associated details.
    /// </summary>
    public interface IVaultLog : IComparable<IVaultLog>
    {
        /// <summary>
        /// Gets the unique identifier of the log.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Gets the log level.
        /// </summary>
        LogLevel Level { get; }

        /// <summary>
        /// Gets the context associated with the log.
        /// </summary>
        string Context { get; }

        /// <summary>
        /// Gets the log message.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets the timestamp (in ticks) when the log was created.
        /// </summary>
        long TimeStampTicks { get; }

        /// <summary>
        /// Gets the stack trace associated with the log.
        /// </summary>
        string Stacktrace { get; }

        /// <summary>
        /// Gets any additional properties associated with the log.
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Initializes the log with the specified details.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="context">The context of the log.</param>
        /// <param name="message">The log message.</param>
        /// <param name="stackTrace">The stack trace for the log.</param>
        /// <param name="properties">Optional additional properties.</param>
        void Init(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null);
    }
}

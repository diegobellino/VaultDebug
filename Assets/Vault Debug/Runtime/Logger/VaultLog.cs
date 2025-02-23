using System;
using System.Collections.Generic;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Represents a log entry with complete details.
    /// </summary>
    public class VaultLog : IVaultLog
    {
        /// <inheritdoc/>
        public long Id { get; private set; }

        /// <inheritdoc/>
        public LogLevel Level { get; private set; }

        /// <inheritdoc/>
        public string Context { get; private set; }

        /// <inheritdoc/>
        public string Message { get; private set; }

        /// <inheritdoc/>
        public long TimeStampTicks { get; private set; }

        /// <inheritdoc/>
        public string Stacktrace { get; private set; }

        /// <inheritdoc/>
        public IDictionary<string, object> Properties { get; private set; }

        private ILogIdProvider _logIdProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultLog"/> class.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="context">The context of the log.</param>
        /// <param name="message">The log message.</param>
        /// <param name="stackTrace">The stack trace associated with the log.</param>
        /// <param name="properties">Optional additional properties.</param>
        /// <param name="id">Optional log ID. If not provided, a new ID is generated.</param>
        public VaultLog(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null, long id = -1)
        {
            _logIdProvider = DIBootstrapper.Container.Resolve<ILogIdProvider>();

            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = id == -1 ? _logIdProvider.GetNextId() : id;
            Properties = properties ?? new Dictionary<string, object>();

            TimeStampTicks = DateTime.Now.Ticks;
        }

        /// <inheritdoc/>
        public void Init(LogLevel level, string context, string message, string stackTrace, IDictionary<string, object> properties = null)
        {
            Level = level;
            Context = context;
            Message = message;
            Stacktrace = stackTrace;
            Id = _logIdProvider.GetNextId();
            Properties = properties ?? new Dictionary<string, object>();

            TimeStampTicks = DateTime.Now.Ticks;
        }

        /// <inheritdoc/>
        public int CompareTo(IVaultLog other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}

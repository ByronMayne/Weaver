using Weaver.Core.Enumerations;

namespace Weaver.Contracts.Diagnostics
{
    public interface ILogEntry
    {
        /// <summary>
        /// Gets the type of log entry that this instance is
        /// </summary>
        LogType Type { get; }

        /// <summary>
        /// Gets the formatted message for this log entry.
        /// </summary>
        string Message { get; }
    }
}

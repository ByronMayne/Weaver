using System;
using System.Collections.Generic;
using System.Text;
using Weaver.Contracts;
using Weaver.Contracts.Diagnostics;
using Weaver.Core.Enumerations;
using Weaver.DataTypes;

namespace Weaver.Diagnostics
{
    /// <summary>
    /// A log entry that has come from a custom Extension
    /// </summary>
    public class AddinLogEntry : ILogEntry
    {
        /// <summary>
        /// Gets the type of log entry that this instance is
        /// </summary>
        public LogType Type { get; }

        /// <summary>
        /// Gets the addin that this entry comes from.
        /// </summary>
        public IWeaverAddin Context { get; }

        /// <summary>
        /// Gets the location where this log is referring too if vaild.
        /// </summary>
        public MemberLocation Location { get; }

        /// <summary>
        /// Gets the formatted message for this log entry.
        /// </summary>
        public string Message { get; }

        public AddinLogEntry(IWeaverAddin context, MemberLocation location, string message, LogType type)
        {
            Type = type;
            Location = location;
            Context = context;
            Message = $"[{type}] {Context.Name}:{location.Line}:{location.Position} | {message}";
        }
    }
}

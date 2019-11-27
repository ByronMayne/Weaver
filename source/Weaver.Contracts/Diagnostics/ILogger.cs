using System;

namespace Weaver.Contracts.Diagnostics
{
    /// <summary>
    /// Used as the logging interface for all operations that happen in Weaver. 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Invoked to create a entry containing information
        /// </summary>
        /// <param name="channel">The channel is the name of the object it belongs too</param>
        /// <param name="message">The message you want in the entry</param>
        void Info(string channel, string message);

        /// <summary>
        /// Invoked to create a entry containing a debug log
        /// </summary>
        /// <param name="channel">The channel is the name of the object it belongs too</param>
        /// <param name="message">The message you want in the entry</param>
        void Debug(string channel, string message);

        /// <summary>
        /// Invoked to create a entry containing a warning
        /// </summary>
        /// <param name="channel">The channel is the name of the object it belongs too</param>
        /// <param name="message">The message you want in the entry</param>
        void Warning(string channel, string message);

        /// <summary>
        /// Invoked to create a entry containing an error
        /// </summary>
        /// <param name="channel">The channel is the name of the object it belongs too</param>
        /// <param name="message">The message you want in the entry</param>
        void Error(string channel, string message);

        /// <summary>
        /// Invoked to create a entry containing an exception
        /// </summary>
        /// <param name="channel">The channel is the name of the object it belongs too</param>
        /// <param name="message">The message you want in the entry</param>
        void Exception(string channel, Exception e); 
    }
}

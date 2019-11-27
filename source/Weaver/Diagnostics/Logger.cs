using System;
using System.Diagnostics;
using Weaver.Contracts.Diagnostics;

namespace Weaver.Diagnostics
{
    public class Logger : ILogger
    {
        /// <summary>
        /// Gets the default instance of a logger. 
        /// </summary>
        public static ILogger Default { get; } = new Logger();

        private Stopwatch m_sessionTimer; 

        private Logger()
        {
            m_sessionTimer = new Stopwatch();
        }

        /// <inheritdoc >
        public void Debug(string channel, string message)
        {
            Log("DBG", channel, message);
        }

        /// <inheritdoc >
        public void Error(string channel, string message)
        {
            Log("ERR", channel, message);
        }

        /// <inheritdoc >
        public void Exception(string channel, Exception e)
        {
            Log("EXC", channel, e.Message + "\n" + e.StackTrace);
        }

        /// <inheritdoc >
        public void Info(string channel, string message)
        {
            Log("INF", channel, message);
        }

        /// <inheritdoc >
        public void Warning(string channel, string message)
        {
            Log("WRN", channel, message);
        }


        /// <summary>Logs the specified type.</summary>
        /// <param name="type">The type.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="message">The message.</param>
        private void Log(string type, string channel, string message)
        {
            TimeSpan time = m_sessionTimer.Elapsed;
            Console.WriteLine($"[{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00} {type}] {channel}: {message}");
        }
    }
}

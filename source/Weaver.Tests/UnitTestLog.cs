using System;
using System.Diagnostics;
using Weaver.Contracts.Diagnostics;

namespace Weaver.Tests
{
    public class UnitTestLog : ILogger
    {
        private Stopwatch m_sessionTimer;

        private UnitTestLog()
        {
            m_sessionTimer = new Stopwatch();
        }

        /// <summary>
        /// Gets the number of debug logs.
        /// </summary>
        public int DebugCount { get; private set; }

        /// <summary>
        /// Gets the number of error logs.
        /// </summary>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// Gets the number of exception logs.
        /// </summary>
        public int ExceptionCount { get; private set; }

        /// <summary>
        /// Gets the number of info logs.
        /// </summary>
        public int InfoCount { get; private set; }

        /// <summary>
        /// Gets the number of warning logs.
        /// </summary>
        public int WarningCount { get; private set; }

        /// <inheritdoc >
        public void Debug(string channel, string message)
        {
            Log("DBG", channel, message);
            DebugCount++;
        }

        /// <inheritdoc >
        public void Error(string channel, string message)
        {
            Log("ERR", channel, message);
            ErrorCount++;
        }

        /// <inheritdoc >
        public void Exception(string channel, Exception e)
        {
            Log("EXC", channel, e.Message + "\n" + e.StackTrace);
            ExceptionCount++;
        }

        /// <inheritdoc >
        public void Info(string channel, string message)
        {
            Log("INF", channel, message);
            InfoCount++;
        }

        /// <inheritdoc >
        public void Warning(string channel, string message)
        {
            Log("WRN", channel, message);
            WarningCount++;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            WarningCount = 0;
            ErrorCount = 0;
            ExceptionCount = 0;
            ErrorCount = 0;
            DebugCount = 0;
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

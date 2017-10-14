using System;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine;
using UnityEditor;

namespace Weaver
{
    [Serializable]
    public class Log
    {
        [Serializable]
        public struct Entry
        {
            public static int selectedID;
            public int id;
            public int lineNumber;
            public string fileName;
            public string message;
            public MessageType type;
        }

        private ILogable m_Context;
        [SerializeField]
        private List<Entry> m_Entries;

        public List<Entry> entries
        {
            get { return m_Entries; }
        }

        public ILogable context
        {
            get { return m_Context; }
            set { m_Context = value; }
        }
        /// <summary>
        /// Creates a new instance of a log.
        /// </summary>
        public Log(ILogable context)
        {
            m_Context = context;
            m_Entries = new List<Entry>();
        }

        public void Clear()
        {
            m_Entries.Clear();
        }

        /// <summary>
        /// Logs a message to the weaver settings log with an
        /// option to write to the Unity console. 
        /// </summary>
        /// <param name="message">The message you want to write</param>
        /// <param name="logToConsole">If true will also log to the Unity console</param>
        public void Info(string message, bool logToConsole)
        {
            string output = FormatLabel(message, MessageType.Info);
            AddEntry(message, MessageType.Info);
            if (logToConsole)
            {
                Debug.Log(output);
            }
        }

        /// <summary>
        /// Logs a warning to the weaver settings log with an
        /// option to write to the Unity console. 
        /// </summary>
        /// <param name="warning">The message you want to write</param>
        /// <param name="logToConsole">If true will also log to the Unity console</param>
        public void Warning(string warning, bool logToConsole)
        {
            string output = FormatLabel(warning, MessageType.Warning);
            AddEntry(warning, MessageType.Warning);
            if (logToConsole)
            {
                Debug.LogWarning(output);
            }
        }

        /// <summary>
        /// Logs a error to the weaver settings log with an
        /// option to write to the Unity console. 
        /// </summary>
        /// <param name="message">The message you want to write</param>
        /// <param name="logToConsole">If true will also log to the Unity console</param>
        public void Error(string error, bool logToConsole)
        {
            string output = FormatLabel(error, MessageType.Error);
            AddEntry(error, MessageType.Error);
            if (logToConsole)
            {
                Debug.LogError(output);
            }
        }

        /// <summary>
        /// Adds the label to the front of the console log.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string FormatLabel(string text, MessageType logType)
        {
            switch (logType)
            {
                case MessageType.Warning:
                    return string.Format("<color=yellow>[{0}]: {1}</color>", m_Context.label, text);
                case MessageType.Error:
                    return string.Format("<color=red>[{0}]: {1}</color>", m_Context.label, text);
            }
            return string.Format("[{0}]: {1}", m_Context.label, text);
        }

        private void AddEntry(string message, MessageType logType)
        {
            // Get our stack frame
            StackFrame frame = new StackFrame(2, true);
            // Create our entry
            Entry entry = new Entry()
            {
                fileName = frame.GetFileName(),
                lineNumber = frame.GetFileLineNumber(),
                message = message,
                type = logType,
                id = m_Entries.Count + 1
            };
            m_Entries.Add(entry);
        }
    }
}

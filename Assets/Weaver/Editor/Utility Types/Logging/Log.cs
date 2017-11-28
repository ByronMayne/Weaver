using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
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
        [UsedImplicitly]
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
            AddEntry(message, MessageType.Info);
            if (logToConsole)
            {
                Debug.Log(message);
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
            AddEntry(warning, MessageType.Warning);
            if (logToConsole)
            {
                Debug.LogWarning(warning);
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
            AddEntry(error, MessageType.Error);
            if (logToConsole)
            {
                Debug.LogError(error);
            }
        }

        /// <summary>
        /// Adds the label to the front of the console log.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string FormatLabel(string message, string fileName, int lineNumber, MessageType logType)
        {
            switch (logType)
            {
                case MessageType.Warning:
                    return string.Format("<color=yellow>[{0}:{1}]: {2}</color>", fileName, lineNumber, message);
                case MessageType.Error:
                    return string.Format("<color=red>[{0}:{1}]: {2}</color>", fileName, lineNumber, message);
            }
            return string.Format("[{0}:{1}]: {2}", fileName, lineNumber, message);
        }

        private void AddEntry(string message, MessageType logType)
        {
            // Get our stack frame
            StackFrame frame = new StackFrame(2, true);
            // Create our entry
            string prettyFileName = System.IO.Path.GetFileNameWithoutExtension(frame.GetFileName());
            int lineNumber = frame.GetFileLineNumber();
            message = FormatLabel(message, prettyFileName, frame.GetFileLineNumber(), logType);
            Entry entry = new Entry()
            {
                fileName = frame.GetFileName(),
                lineNumber = lineNumber,
                message = message,
                type = logType,
                id = m_Entries.Count + 1
            };
            m_Entries.Add(entry);
        }
    }
}

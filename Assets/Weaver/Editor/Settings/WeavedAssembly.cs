using System;
using System.IO;
using UnityEngine;

namespace Weaver
{
    /// <summary>
    /// Keeps track of the assembly path and if the
    /// weaving is enabled or not.
    /// </summary>
    [Serializable]
    public class WeavedAssembly
    {
        [SerializeField]
        private string m_FilePath;
        [SerializeField]
        private bool m_Enabled;
        [SerializeField]
        private int m_LastWriteTime; 

        private FileSystemWatcher m_Watcher;
        private bool m_IsValid;
        private bool m_HasChanged = false;

        /// <summary>
        /// Returns back true if the assembly is
        /// valid and false if it's not. 
        /// </summary>
        public bool isValid
        {
            get { return m_IsValid; }
        }

        public string filePath
        {
            get { return m_FilePath; }
            set { m_FilePath = value; }
        }

        public bool enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        public void Initialize()
        {
            if (m_Watcher == null)
            {
                string directory = Path.GetDirectoryName(filePath);
                string assemblyName = Path.GetFileName(filePath);
                if (File.Exists(filePath))
                {
                    m_Watcher = new FileSystemWatcher(directory, assemblyName);
                    m_Watcher.IncludeSubdirectories = false;
                    m_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                    m_IsValid = true;
                    int writeTime = File.GetLastWriteTime(filePath).Second;
                    if(m_LastWriteTime != writeTime)
                    {
                        m_HasChanged = true;
                        m_LastWriteTime = writeTime;
                    }
                    else
                    {
                        m_HasChanged = false;
                    }
                }
            }
            else
            {
                m_IsValid = false;
            }
        }

        public void AddListener(FileSystemEventHandler handler)
        {
            if (isValid)
            {
                m_Watcher.Changed += handler;

                if(m_HasChanged)
                {
                    string directory = Path.GetDirectoryName(filePath);
                    string assemblyName = Path.GetFileName(filePath);
                    FileSystemEventArgs fileChangedArg = new FileSystemEventArgs(WatcherChangeTypes.Changed, directory, assemblyName);
                    handler.Invoke(this, fileChangedArg);
                }
            }
        }

        public void RemoveListener(FileSystemEventHandler handler)
        {
            if (isValid)
            {
                m_Watcher.Changed -= handler;
            }
        }
    }
}

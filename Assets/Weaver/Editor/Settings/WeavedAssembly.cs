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
        private FileSystemWatcher m_Watcher;
        private bool m_IsValid;

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

        public void AddListener(FileSystemEventHandler handler)
        {
            if (m_Watcher == null)
            {
                string directory = Path.GetDirectoryName(filePath);
                string assemblyName = Path.GetFileName(filePath);
                if (File.Exists(filePath))
                {
                    m_Watcher = new FileSystemWatcher(directory, assemblyName);
                    m_Watcher.Changed += handler;
                }
                else
                {
                    m_IsValid = false;
                }
            }

        }

        public void RemoveListener(FileSystemEventHandler handler)
        {
            if (m_Watcher != null)
            {
                m_Watcher.Changed -= handler;
            }
        }
    }
}

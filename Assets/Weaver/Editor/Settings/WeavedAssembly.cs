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
        public delegate void AssemblyChangedDelegate(string filePath); 

        [SerializeField]
        private string m_FilePath;
        [SerializeField]
        private bool m_Enabled;
        [SerializeField]
        private int m_LastWriteTime; 

        private FileSystemWatcher m_Watcher;
        private bool m_IsValid;
        private bool m_HasChanged = false;
        private AssemblyChangedDelegate m_OnChanged; 

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
                    m_Watcher.Changed += OnAssemblyChanged; 
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

        private void OnAssemblyChanged(object sender, FileSystemEventArgs e)
        {
            InvokeChangedEvent(); 
        }

        private void InvokeChangedEvent()
        {
            if (enabled)
            {
                if (m_OnChanged != null)
                {
                    m_OnChanged(m_FilePath);
                }
            }
        }

        public void AddListener(AssemblyChangedDelegate listener)
        {
            m_OnChanged += listener;

            if (m_HasChanged && m_Enabled)
            {
                listener(m_FilePath); 
            }
        }

        public void RemoveListener(AssemblyChangedDelegate listner)
        {
            m_OnChanged -= listner;
        }
    }
}

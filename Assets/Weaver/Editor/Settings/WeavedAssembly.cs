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
        public delegate void WeavedAssemblyDelegate(WeavedAssembly weavedAssembly);

        [SerializeField]
        private string m_FilePath;
        [SerializeField]
        private bool m_Enabled;
        [SerializeField]
        private int m_LastWriteTime;

        private bool m_IsValid;

        /// <summary>
        /// Returns back true if the assembly is
        /// valid and false if it's not. 
        /// </summary>
        public bool isValid
        {
            get { return m_IsValid; }
        }

        /// <summary>
        /// Returns back the file path to this assembly
        /// </summary>
        public string filePath
        {
            get { return m_FilePath; }
            set { m_FilePath = value; }
        }

        /// <summary>
        /// Returns true if this assembly should be modified
        /// by Weaver or not. 
        /// </summary>
        public bool enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        /// <summary>
        /// Initialize this instance and sets all relevant flags.
        /// </summary>
        public void CheckForChanges(WeavedAssemblyDelegate ifChanged)
        {
            if (File.Exists(filePath))
            {
                m_IsValid = true;
                int writeTime = File.GetLastWriteTime(filePath).Second;
                if (m_LastWriteTime != writeTime)
                {
                    m_LastWriteTime = writeTime;
                    if (ifChanged != null)
                    {
                        ifChanged(this); 
                    }
                }
            }
            else
            {
                m_IsValid = false;
            }
        }
    }
}

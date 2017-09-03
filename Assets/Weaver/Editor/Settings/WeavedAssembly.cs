using System;
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
    }
}

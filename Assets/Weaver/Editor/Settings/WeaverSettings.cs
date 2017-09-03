using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace Weaver
{
    [CreateAssetMenu(fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject // SerializedWeaver<WeaverSettings>
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        // Watchers
        private List<FileSystemWatcher> m_AssemblyWatches;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
        }

        private void OnEnable()
        {
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                m_WeavedAssemblies[i].Initialize();
                m_WeavedAssemblies[i].AddListener(OnAssemblyChanged);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                m_WeavedAssemblies[i].RemoveListener(OnAssemblyChanged);
            }
        }

        /// <summary>
        /// Invoked when ever one of the assemblies we are
        /// watching has changed. 
        /// </summary>
        private void OnAssemblyChanged(object sender, FileSystemEventArgs e)
        {
            // Save our write times to disk.
            EditorUtility.SetDirty(this);
            // Search for our attributes
            AttributeFinder.SerachAssembly(e.FullPath);
        }
    }
}

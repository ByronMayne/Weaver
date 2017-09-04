using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Mono.Cecil;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Settings", fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject // SerializedWeaver<WeaverSettings>
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        [SerializeField]
        private List<WeaverPlugin> m_Addins;

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
        private void OnAssemblyChanged(string filePath)
        {
            // Save our write times to disk.
            EditorUtility.SetDirty(this);
            // Load our definition
            ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(filePath);
            // Make sure it's not null
            if (moduleDefinition != null)
            {
                // Call init on each of our addins
                for (int i = 0; i < m_Addins.Count; i++)
                {
                    if (m_Addins[i] != null)
                    {
                        m_Addins[i].Initialize(moduleDefinition);
                    }
                }
            }

            // Write the values to disk
            moduleDefinition.Write(filePath);
        }
    }
}

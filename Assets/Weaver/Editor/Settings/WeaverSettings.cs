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
            //string[] guids = AssetDatabase.FindAssets("t:WeaverSettings"); 
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

        private void OnAssemblyChanged(object sender, FileSystemEventArgs e)
        {
            // Save our write times to disk.
            EditorUtility.SetDirty(this);
            Debug.Log("Assembly Changed: " + sender + " : " + e.FullPath);
        }
    }
}

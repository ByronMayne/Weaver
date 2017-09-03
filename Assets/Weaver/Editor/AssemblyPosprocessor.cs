using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections.Generic;
namespace Weaver
{
    [InitializeOnLoad]
    public static class AssemblyPosprocessor
    {
        private static List<FileSystemWatcher> m_AssemblyWatcheers;

        static AssemblyPosprocessor()
        {
            ICollection<string> assemblies = AssemblyUtility.GetUserAssemblies(); 

            foreach(string assembly in assemblies)
            {
                Debug.Log("Assembly:  " + assembly);
            }
        }
    }
}

using Mono.Cecil;
using System;
using System.IO;
using UnityEditorInternal;

namespace Weaver
{
    /// <summary>
    /// Used to resolve any references to the Unity Editor or UnityEngine assemblies.
    /// </summary>
    public class WeaverAssemblyResolver : DefaultAssemblyResolver
    {
        private const string UNITY_PREFIX = "Unity";

        private readonly string _unityAssembliesDirectory;

        public WeaverAssemblyResolver()
        {
            // Get the location of the core dll ([ProjectRoot]/Library/UnityAssemblies) 
            string coreAssemblyPath = InternalEditorUtility.GetEngineCoreModuleAssemblyPath();
            // Get the directory name
            _unityAssembliesDirectory = Path.GetDirectoryName(coreAssemblyPath);
        }

        public override AssemblyDefinition Resolve(string fullName)
        {
            if (fullName.StartsWith(UNITY_PREFIX))
            {
                return GetUnityAssemblyDefintion(fullName);
            }

            return base.Resolve(fullName);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (name.FullName.StartsWith(UNITY_PREFIX))
            {
                return GetUnityAssemblyDefintion(name.FullName);
            }

            return base.Resolve(name);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name.FullName.StartsWith(UNITY_PREFIX))
            {
                return GetUnityAssemblyDefintion(name.FullName);
            }

            return base.Resolve(name, parameters);
        }

        public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            if (fullName.StartsWith(UNITY_PREFIX))
            {
                return GetUnityAssemblyDefintion(fullName);
            }

            return base.Resolve(fullName, parameters);
        }

        private AssemblyDefinition GetUnityAssemblyDefintion(string strongName)
        {
            // Example input: "UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
            // Get the starting index 
            int index = strongName.IndexOf(',');
            // Split the start
            strongName = strongName.Substring(0, index);
            // Guess the path (it's always UnityEngine.UI.dll or something) 
            string path = Path.Combine(_unityAssembliesDirectory, strongName + ".dll");
            // If it does not exist 
            if (!File.Exists(path))
            {
                // Quite
                return null;
            }
            // Load it
            return AssemblyDefinition.ReadAssembly(path); 
        }
    }
}

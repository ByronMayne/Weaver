using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Weaver
{
    public class AssemblyUtility
    {
        private static IList<Assembly> m_Assemblies;
        
        /// <summary>
        /// Returns the cached array of user assemblies. If you wan to refresh
        /// call <see cref="PopulateAssemblyCache"/>
        /// </summary>
        /// <returns></returns>
        public static IList<Assembly> GetUserCachedAssemblies()
        {
            return m_Assemblies;
        }

        /// <summary>
        /// Populates our list of loaded assemblies
        /// </summary>
        public static void PopulateAssemblyCache()
        {
            IList<string> assemblyPaths = GetUserAssemblyPaths();
            m_Assemblies = new Assembly[assemblyPaths.Count];
            for(int i = 0;  i < assemblyPaths.Count; i++)
            {
                m_Assemblies[i] = Assembly.LoadFile(assemblyPaths[i]);
            }
        }

        /// <summary>
        /// Forces Unity to recompile all scripts and then refresh. 
        /// </summary>
        /// 
        [MenuItem("CONTEXT/WeaverSettings/Re-weave Assemblies")]
        public static void DirtyAllScripts()
        {                   
#if UNITY_2019_3_OR_NEWER
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
#else
            // Grab the UnityEditor assembly
            Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
            // Find the type that contains the method we want 
            Type compilationInterface = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            // Make sure it's not null 
            if (compilationInterface != null)
            {
                // Create our binding flags
                BindingFlags staticBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                // Grab the dirty method 
                MethodInfo dirtyAllScriptsMethod = compilationInterface.GetMethod("DirtyAllScripts", staticBindingFlags);
                // Invoke the static method with no arguments.
                dirtyAllScriptsMethod.Invoke(null, null);
            }
#endif
            // Force the database to refresh.
            UnityEditor.AssetDatabase.Refresh();
        }
        }

        /// <summary>
        /// Looks over all cached user assemblies for all types that inherit from
        /// the sent in generic. 
        /// </summary>
        public static IList<Type> GetInheirtingTypesFromUserAssemblies<T>()
        {
            IList<Type> result = new List<Type>();
            foreach(Assembly assembly in m_Assemblies)
            {
                foreach(Type type in assembly.GetTypes())
                {
                    if(!type.IsAbstract && typeof(T).IsAssignableFrom(type))
                    {
                        result.Add(type);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns back true if the dll at the path is a managed dll.
        /// </summary>
        /// <param name="systemPath">The full system path to the dll.</param>
        /// <returns>True if a managed dll and false if not. </returns>
        public static bool IsManagedAssembly(string systemPath)
        {
            DllType dllType = InternalEditorUtility.DetectDotNetDll(systemPath);
            return dllType != DllType.Unknown && dllType != DllType.Native;
        }

        /// <summary>
        /// Returns back all the user assemblies define in the unity project. 
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetUserAssemblyPaths()
        {
            List<string> assemblies = new List<string>(20);
            FindAssemblies(Application.dataPath, 120, assemblies);
            FindAssemblies(Application.dataPath + "/../Library/ScriptAssemblies/", 2, assemblies);
            return assemblies;
        }

        /// <summary>
        /// Returns back all the assemblies that are generated in the ScriptAssemblies
        /// folder.
        /// </summary>
        public static IList<string> GetUnityUserGeneratedAssemblyPaths()
        {
            List<string> assemblies = new List<string>(20);
            FindAssemblies(Application.dataPath + "/../Library/ScriptAssemblies/", 2, assemblies);
            return assemblies;
        }

        /// <summary>
        /// Gets a list of all the user Assemblies and returns their
        /// project path. 
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetRelativeUserAssemblyPaths()
        {
            IList<string> assemblies = GetUserAssemblyPaths();
            // Loop over them all
            for(int i = 0; i < assemblies.Count; i++)
            {
                assemblies[i] = FileUtility.SystemToProjectPath(assemblies[i]);
            }
            return assemblies;
        }

        /// <summary>
        /// Finds all the managed assemblies at the give path. It will look into sub folders
        /// up until the max depth. 
        /// </summary>
        /// <param name="systemPath">The path of the directory you want to start looking in.</param>
        /// <param name="maxDepth">The max number of sub directories you want to go into.</param>
        /// <returns></returns>
        public static void FindAssemblies(string systemPath, int maxDepth, List<string> result)
        {
            if (maxDepth > 0)
            {
                try
                {
                    if (Directory.Exists(systemPath))
                    {
                        DirectoryInfo directroyInfo = new DirectoryInfo(systemPath);
                        // Find all assemblies that are managed 
                        result.AddRange(from file in directroyInfo.GetFiles()
                                        where IsManagedAssembly(file.FullName)
                                        select file.FullName);
                        DirectoryInfo[] directories = directroyInfo.GetDirectories();
                        for (int i = 0; i < directories.Length; i++)
                        {
                            DirectoryInfo current = directories[i];
                            FindAssemblies(current.FullName, maxDepth - 1, result);
                        }
                    }
                }
                catch
                {
                    // Nothing to do here
                }
            }
        }
    }
}

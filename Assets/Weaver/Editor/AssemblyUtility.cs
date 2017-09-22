using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace Weaver
{
    public class AssemblyUtility
    {
        private static IList<Assembly> m_Assemblies;
        
        /// <summary>
        /// Retuns the cached array of user assemblies. If you wan to refresh
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
            IList<string> assemblyPaths = GetUserAssemblies();
            m_Assemblies = new Assembly[assemblyPaths.Count];
            for(int i = 0;  i < assemblyPaths.Count; i++)
            {
                m_Assemblies[i] = Assembly.LoadFile(assemblyPaths[i]);
            }
        }
        
        /// <summary>
        /// Looks over all cached user assemblies for all types that inheirt from
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
        /// <param name="path">The full system path to the dll.</param>
        /// <returns>True if a managed dll and false if not. </returns>
        public static bool IsManagedAssembly(string path)
        {
            DllType dllType = InternalEditorUtility.DetectDotNetDll(path);
            return dllType != DllType.Unknown && dllType != DllType.Native;
        }

        /// <summary>
        /// Returns back all the user assemblies define in the unity project. 
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetUserAssemblies()
        {
            List<string> assemblies = new List<string>(20);
            FindAssemblies(Application.dataPath, 1000, assemblies);
            FindAssemblies(Application.dataPath + "/../Library/ScriptAssemblies/", 1000, assemblies);
            return assemblies;
        }

        /// <summary>
        /// Finds all the managed assemblies at the give path. It will look into sub folders
        /// up until the max depth. 
        /// </summary>
        /// <param name="basePath">The path of the directory you want to start looking in.</param>
        /// <param name="maxDepth">The max number of sub directories you want to go into.</param>
        /// <returns></returns>
        public static void FindAssemblies(string basePath, int maxDepth, List<string> result)
        {
            if (maxDepth > 0)
            {
                try
                {
                    if (Directory.Exists(basePath))
                    {
                        DirectoryInfo directroyInfo = new DirectoryInfo(basePath);
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

using Mono.Cecil;
using Seed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Weaver.Contracts;

namespace Weaver
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private IAssemblyCache m_assemblyCache;

        /// <summary>
        /// Contains the paths to every assembly that is currently loaded in our app domain based
        /// on it's named.c
        /// </summary>
        private readonly ISet<AbsolutePath> m_lookupLocations;


        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResolver"/> class.
        /// </summary>
        /// <param name="assemblyCache">The assembly cache.</param>
        public AssemblyResolver(IAssemblyCache assemblyCache)
        {
            m_assemblyCache = assemblyCache;
            m_lookupLocations = new HashSet<AbsolutePath>();

            AppDomain domain = AppDomain.CurrentDomain;
            Assembly[] assemblies = domain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.ReflectionOnly)
                    continue;

                string directory = Path.GetDirectoryName(assembly.Location);
                m_lookupLocations.Add(new AbsolutePath(directory)); 
            }
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return GetAssemblyDefinition(name);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return GetAssemblyDefinition(name);
        }

        private AssemblyDefinition GetAssemblyDefinition(AssemblyNameReference name)
        {
            foreach(AbsolutePath location in m_lookupLocations)
            {
                AbsolutePath assemblyPath = location / $"{name.Name}.dll";
                if(File.Exists(assemblyPath))
                {
                    return m_assemblyCache.Get(assemblyPath, true); 
                }
            }

            return null;
        }

        public void Dispose()
        {
            m_lookupLocations.Clear();
        }
    }
}

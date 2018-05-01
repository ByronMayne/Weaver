using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Weaver
{
    /// <summary>
    /// Used to resolve any references to assemblies that are current in the unity 
    /// project.
    /// </summary>
    public class WeaverAssemblyResolver : BaseAssemblyResolver
    {
        // map of assembly locations
        private readonly IDictionary<string, string> _appDomainAssemblyLocations;
        // cache of loaded AssemblyDefinitions
        private readonly IDictionary<string, AssemblyDefinition> _cache;

        public WeaverAssemblyResolver()
        {
            // Caches
            _appDomainAssemblyLocations = new Dictionary<string, string>();
            _cache = new Dictionary<string, AssemblyDefinition>();
            // Get the current app domain
            AppDomain domain = AppDomain.CurrentDomain;
            // Find all assemblies
            Assembly[] assemblies = domain.GetAssemblies();
            // for each currently loaded assembly,
            for (int i = 0; i < assemblies.Length; i++)
            {
                // store locations
                _appDomainAssemblyLocations[assemblies[i].FullName] = assemblies[i].Location;
                // add all directories as search paths
                AddSearchDirectory(System.IO.Path.GetDirectoryName(assemblies[i].Location));
            }
        }

        public override AssemblyDefinition Resolve(string fullName)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(fullName, null);

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(fullName);
                _cache[fullName] = assemblyDef;
            }

            return assemblyDef;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(name.FullName, null);

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(name);
                _cache[name.FullName] = assemblyDef;
            }

            return assemblyDef;
        }

        public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(fullName, parameters);

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(fullName, parameters);
                _cache[fullName] = assemblyDef;
            }

            return assemblyDef;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(name.FullName, parameters);

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(name, parameters);
                _cache[name.FullName] = assemblyDef;
            }

            return assemblyDef;
        }

        /// <summary>
        /// Searches for AssemblyDefinition in our cache, and failing that,
        /// looks for a known location.  Returns null if both attempts fail.
        /// </summary>
        private AssemblyDefinition FindAssemblyDefinition(string fullName, ReaderParameters parameters)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            // Look in cache first
            AssemblyDefinition assemblyDefinition;
            if (_cache.TryGetValue(fullName, out assemblyDefinition))
            {
                return assemblyDefinition;
            }

            // Try to use known location
            if (_appDomainAssemblyLocations.ContainsKey(fullName))
            {
                string location = _appDomainAssemblyLocations[fullName];

                // Ready the assembly off disk.
                if (parameters != null)
                    assemblyDefinition = AssemblyDefinition.ReadAssembly(location, parameters);
                else
                    assemblyDefinition = AssemblyDefinition.ReadAssembly(location);

                _cache[fullName] = assemblyDefinition;

                return assemblyDefinition;
            }

            return null;
        }
    }
}

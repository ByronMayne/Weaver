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
    public class WeaverAssemblyResolver : DefaultAssemblyResolver
    {
        private readonly IDictionary<string, string> _appDomainAssemblyLocations;

        public WeaverAssemblyResolver()
        {
            // Create a map
            _appDomainAssemblyLocations = new Dictionary<string, string>();
            // Get the current app domain
            AppDomain domain = AppDomain.CurrentDomain;
            // Find all assemblies
            Assembly[] assemblies = domain.GetAssemblies();
            // Loop over all assemblies and populate the map
            for(int i = 0; i < assemblies.Length; i++)
            {
                _appDomainAssemblyLocations[assemblies[i].FullName] = assemblies[i].Location;
            }
        }

        public override AssemblyDefinition Resolve(string fullName)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(fullName);

            if (assemblyDef == null)
            {
             
                assemblyDef = base.Resolve(fullName);
            }

            return assemblyDef;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(name.FullName); 

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(name);
            }

            return assemblyDef;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(name.FullName); 

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(name, parameters);
            }

            return assemblyDef;
        }

        public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            AssemblyDefinition assemblyDef = FindAssemblyDefinition(fullName);

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(fullName, parameters);
            }

            return assemblyDef;
        }

        /// <summary>
        /// Using the assembly map we try to find the assembly and create an Assembly Definition
        /// </summary>
        private AssemblyDefinition FindAssemblyDefinition(string strongName)
        {
            // Try to match their name
            if(_appDomainAssemblyLocations.ContainsKey(strongName))
            {
                string location = _appDomainAssemblyLocations[strongName];
                // Ready the assembly off disk.
                return AssemblyDefinition.ReadAssembly(location);
            }
            return null;
        }
    }
}

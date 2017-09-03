using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Weaver
{
    public class AttributeFinder
    {
        public static void SerachAssembly(string assemblyPath)
        {
            // Get our assembly def
            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
            // Make sure it's not null
            if(assemblyDefinition  == null)
            {
                return;
            }
            // Get our module
            ModuleDefinition mainModule = assemblyDefinition.MainModule;
            // Import our MethodTimerAttribute so we can search for it
            TypeReference methodTimerTypeRef = mainModule.Import(typeof(MethodTimerAttribute));
            // Get all the types
            IEnumerable<TypeDefinition> typeDefinitions = mainModule.GetTypes();
            // Loop over every time
            foreach(TypeDefinition type in typeDefinitions)
            {
                // Get our methods 
                Collection<MethodDefinition> methods = type.Methods;
                // Loop over all methods
                foreach (MethodDefinition method in methods)
                {
                    // Check if we have attributes
                    if (method.HasCustomAttributes)
                    {
                        // Get our custom attributes
                        Collection<CustomAttribute> attributes = method.CustomAttributes;
                        // Loop over all attributes
                        foreach(CustomAttribute attribute in attributes)
                        {
                            // Check if they are the same type
                            if(attribute.AttributeType.FullName.Equals(methodTimerTypeRef.FullName, StringComparison.Ordinal))
                            {
                                // They are so we have a match!
                                UnityEngine.Debug.Log("MATCH!");
                            }
                        }
                    }
                }
            }
        }

        [MethodTimer]
        private Type[] Attributes()
        {
            return new Type[] { typeof(MethodTimerAttribute), typeof(PropertyChangedAttribute) };
        }
    }
}

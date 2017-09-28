using Mono.Cecil;
using System;

namespace Weaver
{
    public static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Finds a method from a type based on it's name. 
        /// </summary>
        public static MethodDefinition GetMethod(this TypeDefinition instance, string name)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDef = instance.Methods[i];

                if (string.Compare(methodDef.Name, name) == 0)
                {
                    return methodDef;
                }
            }
            return null;
        }

        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, params Type[] parameterTypes)
        {
            MethodDefinition result = null;
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDefinition = instance.Methods[i];

                if (string.Equals(methodDefinition.Name, name, StringComparison.Ordinal) // Names Match
                    && parameterTypes.Length == methodDefinition.Parameters.Count) // The same number of parameters
                {
                    result = methodDefinition;
                    for (int x = methodDefinition.Parameters.Count - 1; x >= 0; x--)
                    {
                        ParameterDefinition parameter = methodDefinition.Parameters[x];
                        if (!string.Equals(parameter.ParameterType.Name, parameterTypes[x].Name, StringComparison.Ordinal))
                        {
                            result = null;
                            break;
                        }

                        if (x == 0 && result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, params TypeReference[] parameterTypes)
        {
            MethodDefinition result = null;
            if (instance.Methods != null)
            {
                for (int i = 0; i < instance.Methods.Count; i++)
                {
                    MethodDefinition methodDefinition = instance.Methods[i];
                    if (string.Equals(methodDefinition.Name, name, StringComparison.Ordinal) // Names Match
                        && parameterTypes.Length == methodDefinition.Parameters.Count) // The same number of parameters
                    {
                        result = methodDefinition;
                        for (int x = methodDefinition.Parameters.Count - 1; x >= 0; x--)
                        {
                            ParameterDefinition parameter = methodDefinition.Parameters[x];
                            if (!string.Equals(parameter.ParameterType.Name, parameterTypes[x].Name, StringComparison.Ordinal))
                            {
                                result = null;
                                break;
                            }

                            if (x == 0 && result != null)
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a method from a type based on it's name and argument count
        /// </summary>
        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, int argCount)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDef = instance.Methods[i];

                if (string.Compare(methodDef.Name, name) == 0 && methodDef.Parameters.Count == argCount)
                {
                    return methodDef;
                }
            }
            return null;
        }

        public static PropertyDefinition GetProperty(this TypeDefinition instance, string name)
        {
            for (int i = 0; i < instance.Properties.Count; i++)
            {
                PropertyDefinition preopertyDef = instance.Properties[i];

                // Properties can only have one argument or they are an indexer. 
                if (string.Compare(preopertyDef.Name, name) == 0 && preopertyDef.Parameters.Count == 0)
                {
                    return preopertyDef;
                }
            }
            return null;
        }
    }
}

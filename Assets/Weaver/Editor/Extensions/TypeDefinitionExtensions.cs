using Mono.Cecil;

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
            for (int i = 0; i < instance.Methods.Count; i++)
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

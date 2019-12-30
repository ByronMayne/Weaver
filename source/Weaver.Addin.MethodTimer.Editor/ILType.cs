using Mono.Cecil;
using System;

namespace Weaver.Addin.MethodTimer.Editor
{
    public class ILType
    {
        public readonly TypeReference TypeReference;
        public readonly TypeDefinition TypeDefinition;

        public readonly MethodReference GetTypeFromHandle;

        public ILType(ModuleDefinition moduleDefinition)
        {
            moduleDefinition.ImportFluent<Type>()
                .GetType(out TypeDefinition)
                .GetType(out TypeReference)
                .GetStaticMethod(() => Type.GetTypeFromHandle(new RuntimeTypeHandle()), out GetTypeFromHandle);
        }
    }
}

using Mono.Cecil;
using System;

namespace Weaver
{
    public struct TypeImplementation
    {
        public TypeReference reference;
        public TypeDefinition definition;
        private readonly ModuleDefinition m_Module;

        public TypeImplementation(ModuleDefinition module, Type type)
        {
            m_Module = module;
            reference = m_Module.Import(type);
            definition = reference.Resolve();
        }

        public MethodImplementation GetConstructor()
        {
            return GetMethod(".ctor");
        }

        public MethodImplementation GetConstructor(params Type[] parameterTypes)
        {
            return GetMethod(".ctor", parameterTypes);
        }

        public MethodImplementation GetMethod(string methodName)
        {
            MethodDefinition methodDefinition = definition.GetMethod(methodName);
            MethodImplementation methodImplementation = new MethodImplementation(m_Module,methodDefinition);
            return methodImplementation;
        }

        public MethodImplementation GetMethod(string methodName, params Type[] parameterTypes)
        {
            MethodDefinition methodDefinition = definition.GetMethod(methodName, parameterTypes);
            MethodImplementation methodImplementation = new MethodImplementation(m_Module, methodDefinition);
            return methodImplementation;
        }

        public PropertyImplementation GetProperty(string methodName)
        {
            PropertyDefinition propertyDefinition = definition.GetProperty(methodName);
            PropertyImplementation methodImplementation = new PropertyImplementation(m_Module, propertyDefinition);
            return methodImplementation;
        }
    }
}

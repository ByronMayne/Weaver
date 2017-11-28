using Mono.Cecil;

namespace Weaver
{
    public struct PropertyImplementation
    {
        public PropertyDefinition definition;
        private readonly ModuleDefinition m_Module;

        public PropertyImplementation(ModuleDefinition module, PropertyDefinition propertyDefinition)
        {
            m_Module = module;
            definition = propertyDefinition;
        }

        public MethodImplementation Get()
        {
            MethodDefinition getMethod = definition.GetMethod;
            return new MethodImplementation(m_Module, getMethod);
        }

        public MethodImplementation Set()
        {
            MethodDefinition setMethod = definition.SetMethod;
            return new MethodImplementation(m_Module, setMethod);
        }
    }
}
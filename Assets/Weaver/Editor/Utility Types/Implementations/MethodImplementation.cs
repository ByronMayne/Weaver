using Mono.Cecil;

namespace Weaver
{
    public struct MethodImplementation
    {
        public MethodReference reference;
        public MethodDefinition definition;
        private readonly ModuleDefinition m_Module;

        public MethodImplementation(ModuleDefinition module, MethodDefinition methodDefinition)
        {
            m_Module = module;
            reference = m_Module.Import(methodDefinition);
            definition = reference.Resolve();
        }
    }
}
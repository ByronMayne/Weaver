
using Mono.Cecil;

namespace Weaver
{
    public struct FieldImplementation
    {
        public FieldReference reference;
        public FieldDefinition definition;
        private readonly ModuleDefinition m_Module;

        public FieldImplementation(ModuleDefinition module, FieldDefinition fieldDefinition)
        {
            m_Module = module;
            reference = m_Module.Import(fieldDefinition);
            definition = reference.Resolve();
        }
    }
}

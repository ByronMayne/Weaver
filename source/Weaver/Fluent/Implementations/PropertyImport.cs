using Mono.Cecil;

namespace Weaver.Fluent.Implementations
{
    public struct PropertyImport<T> : IPropertyImport<T>
    {
        private readonly ITypeImport<T> m_parentType;
        private readonly PropertyDefinition m_propertyDefinition;
        private readonly ModuleDefinition m_moduleDefinition;
        private readonly string m_name;
        private readonly bool m_isExternalType;

        public PropertyImport(TypeImport<T> type, ModuleDefinition moduleDefinition, string name, bool isExternalType)
        {
            m_parentType = type;
            m_name = name;
            m_moduleDefinition = moduleDefinition;
            m_parentType.GetType(out TypeDefinition typeDefinition);
            m_propertyDefinition = typeDefinition.GetProperty(m_name);
            m_isExternalType = isExternalType;
        }

        public ITypeImport<T> DeclaringType => m_parentType;

        public IPropertyImport<T> GetGetter(out MethodDefinition methodDefinition)
        {
            methodDefinition = m_propertyDefinition.GetMethod;
            return this;
        }

        public IPropertyImport<T> GetGetter(out MethodReference methodReference)
        {
            methodReference = m_propertyDefinition.GetMethod;
            if (m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodReference);
            }
            return this;
        }

        public IPropertyImport<T> GetSetter(out MethodDefinition methodDefinition)
        {
            methodDefinition = m_propertyDefinition.SetMethod;
            return this;
        }

        public IPropertyImport<T> GetSetter(out MethodReference methodReference)
        {
            methodReference = m_propertyDefinition.SetMethod;
            if(m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodReference);
            }
            return this;
        }

    }
}

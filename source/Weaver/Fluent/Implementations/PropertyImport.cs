using Mono.Cecil;

namespace Weaver.Fluent.Implementations
{
    public struct PropertyImport<T> : IPropertyImport<T>
    {
        private ITypeImport<T> m_parentType;

        public PropertyImport(TypeImport<T> type)
        {
            m_parentType = type;
        }
        public ITypeImport<T> DeclaringType => m_parentType;

        public IPropertyImport<T> GetGetter(out MethodDefinition methodDefinition)
        {
            methodDefinition = null;
            return this;
        }

        public IPropertyImport<T> GetterRef(out MethodReference methodReference)
        {
            methodReference = null;
            return this;
        }

        public IPropertyImport<T> GetSetter(out MethodDefinition methodDefinition)
        {
            methodDefinition = null;
            return this;
        }

        public IPropertyImport<T> SetterRef(out MethodReference methodReference)
        {
            methodReference = null;
            return this;
        }

    }
}

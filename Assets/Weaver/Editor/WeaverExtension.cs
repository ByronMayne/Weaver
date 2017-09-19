using System;
using UnityEngine;
using Mono.Cecil;

namespace Weaver
{
    public enum DefinitionType
    {
        None = 0,
        Module = 1 << 1,
        Type = 1 << 2,
        Method = 1 << 3,
        Field = 1 << 4,
        Property = 1 << 5,
    }

    public abstract class WeaverComponent : ScriptableObject
    {
        private WeaverSettings m_Settings; 
        public abstract string addinName { get; }


        public WeaverSettings settings
        {
            get { return m_Settings; }
            protected set { m_Settings = value; }
        }

        public virtual DefinitionType effectedDefintions { get { return DefinitionType.None; } }

        /// <summary>
        /// Returns true if this addin effects the definition
        /// of a type. 
        /// </summary>
        public bool EffectsDefintion(DefinitionType type)
        {
            return (type & effectedDefintions) == type; 
        }

        public virtual void Initialize(WeaverSettings settings)
        {
            m_Settings = settings;
        }

        public virtual void VisitModule(ModuleDefinition moduleDefinition) { }
        public virtual void VisitType(TypeDefinition typeDefinition) { }
        public virtual void VisitMethod(MethodDefinition methodDefinition) { }
        public virtual void VisitField(FieldDefinition fieldDefinition) { }
        public virtual void VisitProperty(PropertyDefinition propertyDefinition) { }
    }
}

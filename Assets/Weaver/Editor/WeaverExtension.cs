using System;
using UnityEngine;
using Mono.Cecil;
using System.Reflection;

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
        All = Module | Type | Method | Field | Property
    }

    public abstract class WeaverComponent : ScriptableObject
    {
        private ModuleDefinition m_ActiveModule;
        public abstract string addinName { get; }
        private bool m_Enabled;

        public bool enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        /// <summary>
        /// Returns back the type system for the module
        /// currently being edited. If we are not editing a module this
        /// returns null.
        /// </summary>
        public TypeSystem typeSystem
        {
            get { return m_ActiveModule == null ? null : m_ActiveModule.TypeSystem; }
        }

        /// <summary>
        /// Returns back the type of definitions this component modifies. 
        /// </summary>
        public virtual DefinitionType effectedDefintions
        {
            get
            {
                return DefinitionType.None;
            }
        }

        /// <summary>
        /// Returns true if this addin effects the definition
        /// of a type. 
        /// </summary>
        public bool EffectsDefintion(DefinitionType type)
        {
            return (type & effectedDefintions) == type;
        }

        /// <summary>
        /// Invoked whenever we start editing a moudle. Used to populate our
        /// helper functions 
        /// </summary>
        public virtual void OnBeforeModuleEdited(ModuleDefinition moduleDefinition)
        {
            m_ActiveModule = moduleDefinition;
        }

        /// <summary>
        /// Invoked when we have finished editing a module
        /// </summary>
        public virtual void OnModuleEditComplete(ModuleDefinition moduleDefinition)
        {
            m_ActiveModule = null;
        }

        public virtual void VisitModule(ModuleDefinition moduleDefinition) { }
        public virtual void VisitType(TypeDefinition typeDefinition) { }
        public virtual void VisitMethod(MethodDefinition methodDefinition) { }
        public virtual void VisitField(FieldDefinition fieldDefinition) { }
        public virtual void VisitProperty(PropertyDefinition propertyDefinition) { }

        #region -= Import Methods =-
        public TypeReference Import(TypeReference type)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(type);
        }

        public TypeReference Import(Type type, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(type);
        }

        public FieldReference Import(FieldInfo field)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(field);
        }

        public FieldReference Import(FieldInfo field, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(field);
        }

        public MethodReference Import(MethodBase method)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(method);
        }

        public MethodReference Import(MethodBase method, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(method, context);
        }

        public TypeReference Import(TypeReference type, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(type, context);
        }

        public TypeReference Import(Type type)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(type);
        }

        public FieldReference Import(FieldReference field)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(field);
        }

        public MethodReference Import(MethodReference method)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(method);
        }

        public MethodReference Import(MethodReference method, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(method, context);
        }

        public FieldReference Import(FieldReference field, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.Import(field, context);
        }
        #endregion
    }
}

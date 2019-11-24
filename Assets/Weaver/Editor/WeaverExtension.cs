using System;
using UnityEngine;
using Mono.Cecil;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Weaver
{
    [Flags]
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

    public abstract class WeaverComponent : ScriptableObject, ILogable
    {
        // Hidden for now
        [SerializeField, HideInInspector]
        private bool m_IsActive = true;
        [SerializeField, HideInInspector]
        private ScriptingSymbols m_RequiredScriptingSymbols;

        private ModuleDefinition m_ActiveModule;
        public abstract string addinName { get; }
        private Log m_Log;

        public bool isActive
        {
            get { return m_IsActive && m_RequiredScriptingSymbols.isActive; }
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
        /// The context object for our logging. 
        /// </summary>
        public Object context
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Returns back the label we use for our logs.
        /// </summary>
        public string label
        {
            get { return GetType().Name; }
        }

        private void OnEnable()
        {
            m_RequiredScriptingSymbols.ValidateSymbols();
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
        /// Invoked whenever we start editing a module. Used to populate our
        /// helper functions 
        /// </summary>
        public virtual void OnBeforeModuleEdited(ModuleDefinition moduleDefinition, Log log)
        {
            if (m_RequiredScriptingSymbols.isActive)
            {
                m_Log = log;
                m_ActiveModule = moduleDefinition;
                Log("Visiting module");
            }
            else
            {
                Log("Visitation skip as scripting define requirements not met.");
            }
        }

        /// <summary>
        /// Invoked when we have finished editing a module
        /// </summary>
        public virtual void OnModuleEditComplete(ModuleDefinition moduleDefinition)
        {
            m_ActiveModule = null;
            Log("Module visitation complete");
        }

        public virtual void VisitModule(ModuleDefinition moduleDefinition) { }
        public virtual void VisitType(TypeDefinition typeDefinition) { }
        public virtual void VisitMethod(MethodDefinition methodDefinition) { }
        public virtual void VisitField(FieldDefinition fieldDefinition) { }
        public virtual void VisitProperty(PropertyDefinition propertyDefinition) { }

        #region -= Logging =-
        /// <summary>
        /// Logs a message to the console 
        /// </summary>
        protected void Log(string message)
        {
            m_Log.Info(addinName, message, false, 3);
        }

        /// <summary>
        /// Logs a message to the console with a member for context
        /// </summary>
        public void Log(string message, MemberLocation memberContext)
        {
            m_Log.Info(addinName, memberContext + message, false, 3);
        }

        /// <summary>
        /// Logs a warning to the console with a member for context
        /// </summary>
        public void Warning(string message)
        {
            m_Log.Warning(addinName, message, false, 3);
        }

        /// <summary>
        /// Logs a warning to the console with a member for context
        /// </summary>
        public void Warning(string message, MemberLocation memberContext)
        {
            m_Log.Warning(addinName, memberContext + message, false, 3);
        }

        /// <summary>
        /// Logs a error to the console with a member for context
        /// </summary>
        public void Error(string message)
        {
            m_Log.Error(addinName, message, true, 3);
        }

        /// <summary>
        /// Logs a error to the console with a member for context
        /// </summary>
        public void Error(string message, MemberLocation memberContext)
        {
            m_Log.Error(addinName, memberContext + message, false, 3);
        }
        #endregion

        #region -= Import Methods =-
        public TypeReference Import(TypeReference type)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(type);
        }

        public TypeReference Import(Type type, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(type);
        }

        public FieldReference Import(FieldInfo field)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(field);
        }

        public FieldReference Import(FieldInfo field, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(field);
        }

        public MethodReference Import(MethodBase method)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(method);
        }

        public MethodReference Import(MethodBase method, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(method, context);
        }

        public TypeReference Import(TypeReference type, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(type, context);
        }

        public TypeReference Import(Type type)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(type);
        }

        public FieldReference Import(FieldReference field)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(field);
        }

        public MethodReference Import(MethodReference method)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(method);
        }

        public MethodReference Import(MethodReference method, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(method, context);
        }

        public FieldReference Import(FieldReference field, IGenericParameterProvider context)
        {
            if (m_ActiveModule == null)
            {
                return null;
            }
            return m_ActiveModule.ImportReference(field, context);
        }
        #endregion
    }
}

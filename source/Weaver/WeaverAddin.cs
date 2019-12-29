using Mono.Cecil;
using System;
using System.Reflection;
using Weaver.Contracts;
using Weaver.Contracts.Diagnostics;
using Weaver.Core;

namespace Weaver
{
    public abstract class WeaverAddin : IWeaverAddin
    {
        public abstract string Name { get; }

        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the affected definitions. Only the Visit methods of the
        /// types returned by this will be invoked.
        /// </summary>
        public DefinitionType AffectedDefinitions { get; private set; }

        public TypeSystem TypeSystem { get; private set; }

        public WeaverAddin()
        {
            Type classType = GetType();


            // Used to set our AffectedDefinitions flag if the funtions are overloaded 
            void CheckOverride(string methodName, DefinitionType type)
            {
                MethodInfo methodDefinition = classType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

                if (methodDefinition != null && methodDefinition.DeclaringType == classType)
                {
                    AffectedDefinitions |= type;
                }
            }


            CheckOverride(nameof(VisitModule), DefinitionType.Modules);
            CheckOverride(nameof(VisitAssembly), DefinitionType.Assembly);
            CheckOverride(nameof(VisitEvent), DefinitionType.Event);
            CheckOverride(nameof(VisitField), DefinitionType.Field);
            CheckOverride(nameof(VisitMethod), DefinitionType.Method);
            CheckOverride(nameof(VisitProperty), DefinitionType.Property);
            CheckOverride(nameof(VisitType), DefinitionType.Type);
        }



        /// <summary>
        /// Invoked whenver we enter the specified assembly definition.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void VisitAssembly(AssemblyDefinition assemblyDefinition)
        {}

        /// <summary>
        /// I
        /// nvoked whenver we enter the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void VisitModule(ModuleDefinition moduleDefinition)
        {
            TypeSystem = moduleDefinition.TypeSystem;
        }

        /// <summary>
        /// Invoked whenver we enter the specified type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void VisitType(TypeDefinition typeDefinition)
        {}

        /// <summary>
        /// Invoked whnever we enter the specified method definition.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void VisitMethod(MethodDefinition methodDefinition)
        {}

        /// <summary>
        /// Invoked whenver we enter the specified property definition.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void VisitProperty(PropertyDefinition propertyDefinition)
        {}

        /// <summary>
        /// Invoked whenver we enter the specified field definition.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void VisitField(FieldDefinition fieldDefinition)
        {}

        /// <summary>
        /// Invoked whenver we enter the specified event definition.
        /// </summary>
        /// <param name="eventDefinition"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void VisitEvent(EventDefinition eventDefinition)
        {}
    }
}

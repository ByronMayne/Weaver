using Mono.Cecil;
using Mono.Collections.Generic;
using UnityEngine;

namespace Weaver
{
    [System.Serializable]
    public class ComponentController : SubObjectController<WeaverComponent>
    {
        /// <summary>
        /// This is used to only loop over definition types that
        /// we are using. 
        /// </summary>
        private DefinitionType m_ActiveDefinitions;

        /// <summary>
        /// Sets up the component controller. 
        /// </summary>
        public void Initialize(Object owner)
        {
            m_Owner = owner;
            for (int i = 0; i < m_SubObjects.Count; i++)
            {
                m_ActiveDefinitions |= m_SubObjects[i].effectedDefintions;
            }
        }

        /// <summary>
        /// Takes in a module and invokes <see cref="WeaverComponent.VisitModule(ModuleDefinition)"/> 
        /// on all components. 
        /// </summary>
        public void VisitModules(Collection<ModuleDefinition> moduleCollection)
        {
            if (m_ActiveDefinitions != DefinitionType.None)
            {
                for (int moduleIndex = moduleCollection.Count - 1; moduleIndex >= 0; moduleIndex--)
                {
                    // Grab the type system for the current moudle.
                    TypeSystem typeSystem = moduleCollection[moduleIndex].TypeSystem;
                    // Loop over all sub objects
                    for (int componentIndex = m_SubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                    {
                        // Assign our type system
                        m_SubObjects[componentIndex].typeSystem = typeSystem; 
                        // Loop over modules if we are editing them 
                        if ((m_ActiveDefinitions & DefinitionType.Module) != DefinitionType.Module)
                        {
                            m_SubObjects[componentIndex].VisitModule(moduleCollection[moduleIndex]);
                        }
                    }
                    // Viste Types
                    VisitTypes(moduleCollection[moduleIndex].Types);
                }
            }
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="WeaverComponent.VisitType(TypeDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitTypes(Collection<TypeDefinition> typeCollection)
        {
            // We only don't have to visit types if nobody vistes properties, methods, or fields. 
            if ((m_ActiveDefinitions & ~DefinitionType.Module) != DefinitionType.None)
            {
                for (int typeIndex = typeCollection.Count - 1; typeIndex >= 0; typeIndex--)
                {
                    for (int componentIndex = m_SubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                    {
                        m_SubObjects[componentIndex].VisitType(typeCollection[typeIndex]);
                    }
                    // Viste Methods
                    VisitMethods(typeCollection[typeIndex].Methods);
                    // Viste Fields
                    VisitFields(typeCollection[typeIndex].Fields);
                    // Viste Properties
                    VisitProperties(typeCollection[typeIndex].Properties);
                }
            }
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="WeaverComponent.VisitMethod(MethodDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitMethods(Collection<MethodDefinition> methodCollection)
        {
            // Only vist methods if we have any components that modify them.
            if ((m_ActiveDefinitions & DefinitionType.Method) == DefinitionType.Method)
            {
                for (int methodIndex = methodCollection.Count - 1; methodIndex >= 0; methodIndex--)
                {
                    for (int componentIndex = m_SubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                    {
                        m_SubObjects[componentIndex].VisitMethod(methodCollection[methodIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="WeaverComponent.VisitField(FieldDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitFields(Collection<FieldDefinition> fieldCollection)
        {
            // Only vist fields if we have any components that modify them.
            if ((m_ActiveDefinitions & DefinitionType.Field) == DefinitionType.Field)
            {
                for (int fieldIndex = fieldCollection.Count - 1; fieldIndex >= 0; fieldIndex--)
                {
                    for (int componentIndex = m_SubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                    {
                        m_SubObjects[componentIndex].VisitField(fieldCollection[fieldIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="WeaverComponent.VisitField(FieldDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitProperties(Collection<PropertyDefinition> propertyCollection)
        {
            // Only vist properties if we have any components that modify them.
            if ((m_ActiveDefinitions & DefinitionType.Property) == DefinitionType.Property)
            {
                for (int propertyIndex = propertyCollection.Count - 1; propertyIndex >= 0; propertyIndex--)
                {
                    for (int componentIndex = m_SubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                    {
                        m_SubObjects[componentIndex].VisitProperty(propertyCollection[propertyIndex]);
                    }
                }
            }
        }
    }
}

using System;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Weaver
{
    public class PropertyChangedComponent : WeaverComponent
    {
        public override string addinName
        {
            get
            {
                return "Property Changed";
            }
        }

        public override DefinitionType effectedDefintions
        {
            get
            {
                return DefinitionType.Module | DefinitionType.Property;
            }
        }

        public override void VisitModule(ModuleDefinition moduleDefinition)
        {
            base.VisitModule(moduleDefinition);
        }

        public override void VisitProperty(PropertyDefinition propertyDefinition)
        {
            // Attempt to get our attribute 
            CustomAttribute customAttribute = propertyDefinition.GetCustomAttribute<OnChangedAttribute>();
            // Check if it's null
            if (customAttribute == null)
            {
                // No attritbute so skip it
                return;
            }
            // Get our set method
            MethodReference setMethod = propertyDefinition.SetMethod;
            // If we don't have a setter this does nothing
            if(setMethod == null)
            {
                Warning("OnChangedAttribute was applied to but no setter was defined. This attribute wil have no effect", propertyDefinition.DeclaringType.GetLocation());
                return;
            }
            // Get the name of the method we are going to connect
            string callbackMethod = customAttribute.GetValue<string>("callbackMethod");
            // Get the argument type we are looking for
            TypeReference propertyType = propertyDefinition.PropertyType;
            // Look for the method
            MethodReference callbackWithArg = propertyDefinition.DeclaringType.GetMethod(callbackMethod, propertyType);
            // Check if we found one
            if(callbackWithArg == null)
            {
                Error("Unable to find callback method '" + callbackMethod + "' in the defined class. No callback will be bound to this property.", setMethod.Resolve().GetLocation());
                return;
            }
        }
    }
}

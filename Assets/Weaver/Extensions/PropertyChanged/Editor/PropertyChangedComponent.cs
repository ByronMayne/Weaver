using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Weaver.Extensions;

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
            MethodReference getMethod = propertyDefinition.GetMethod;
            // If we don't have a setter this does nothing
            if (setMethod == null)
            {
                Warning("OnChangedAttribute was applied to " + propertyDefinition.Name + " but no setter was defined. This attribute wil have no effect", propertyDefinition.DeclaringType.GetLocation());
                return;
            }
            if (getMethod == null)
            {
                Warning("OnChangedAttribute was applied to the " + propertyDefinition.Name + " and no getter was defined. This attribute wil have no effect", setMethod.Resolve().GetLocation());
                return;
            }
            // Get the name of the method we are going to connect
            string callbackMethod = (string)customAttribute.ConstructorArguments[0].Value;
            // Get the argument type we are looking for
            TypeReference propertyType = propertyDefinition.PropertyType;
            // Look for the method
            MethodReference callbackWithArg = propertyDefinition.DeclaringType.GetMethod(callbackMethod, propertyType);
            // Check if we found one
            if (callbackWithArg == null)
            {
                Error("Unable to find callback method '" + callbackMethod + "' in the defined class. No callback will be bound to this property.", setMethod.Resolve().GetLocation());
                return;
            }

            // Inject Property Functions 
            try
            {
                MethodDefinition setMethodDefinition = setMethod.Resolve();
                MethodBody methodBody = setMethodDefinition.Body;
                ILProcessor ilProcessor = methodBody.GetILProcessor();
                Instruction _start = methodBody.Instructions[0];
                // IL Time
                Instruction _01 = ilProcessor.Create(OpCodes.Nop);
                Instruction _02 = ilProcessor.Create(OpCodes.Ldarg_0);
                Instruction _03 = ilProcessor.Create(OpCodes.Call, getMethod);
                Instruction _04 = ilProcessor.Create(OpCodes.Ldarg_1);
                Instruction _05 = ilProcessor.Create(OpCodes.Beq, _start);

                // If True
                Instruction _06 = ilProcessor.Create(OpCodes.Nop);
                Instruction _07 = ilProcessor.Create(OpCodes.Ldarg_0);
                Instruction _08 = ilProcessor.Create(OpCodes.Ldarg_1);
                Instruction _10 = ilProcessor.Create(OpCodes.Call, callbackWithArg);
                Instruction _11 = ilProcessor.Create(OpCodes.Nop);
                // Grab the first one 

                ilProcessor.InsertBefore(_start, _01);
                ilProcessor.InsertAfter(_01, _02);
                ilProcessor.InsertAfter(_02, _03);
                ilProcessor.InsertAfter(_03, _04);
                ilProcessor.InsertAfter(_04, _05);

                ilProcessor.InsertAfter(_05, _06);
                ilProcessor.InsertAfter(_06, _07);
                ilProcessor.InsertAfter(_07, _08);
                ilProcessor.InsertAfter(_08,  _10);
                ilProcessor.InsertAfter(_10, _11);

                // Remove out attribute to clean up (since at this point we successfuly wrote).
                propertyDefinition.CustomAttributes.Remove(customAttribute);
            }
            catch (Exception e)
            {
                Error(e.ToString());
                return;
            }
            /*
            // Check for validation 
            bool isValidated = customAttribute.GetValue<bool>("isValidated");
            // Do it only if true
            if(isValidated)
            {
                // Add a new field to hold our last state 
                FieldDefinition validationField = new FieldDefinition("___Validation_" + propertyDefinition.Name, FieldAttributes.Private, propertyDefinition.PropertyType);
                // Look for the OnValidateFunction
                MethodDefinition onValidateFunction = propertyDefinition.DeclaringType.GetMethod("OnValidate", 0);
                // Check if it's null
                if(onValidateFunction == null)
                {
                    Error("No OnValidate function was found in type " + propertyDefinition.DeclaringType.Name + " so the validation flag on property " +
                        propertyDefinition.Name + " will have no effect. Please at the method or remove the flag.", propertyDefinition.GetMethod.GetLocation());
                }
                // Get our fileds
                MethodBody body = onValidateFunction.Body;
                ILProcessor ilProcessor = body.GetILProcessor();

                Instruction start = body.Instructions[0];

                // if( __valdiation != getValue )
                Instruction _01 = ilProcessor.Create(OpCodes.Nop);
                Instruction _02 = ilProcessor.Create(OpCodes.Ldarg_0);
                Instruction _03 = ilProcessor.Create(OpCodes.Ldfld, validationField);
                Instruction _04 = ilProcessor.Create(OpCodes.Ldarg_1);
                Instruction _05 = ilProcessor.Create(OpCodes.Ldfld, getMethod);
                Instruction _06 = ilProcessor.Create(OpCodes.Beq, start);
                //{
                Instruction _07 = ilProcessor.Create(OpCodes.Nop);
                //Instruction _08 = ilProcessor.Create(OpCodes.Ldarg_0);
                //Instruction _09 = ilProcessor.Create(OpCodes.Ldarg_1);
                //Instruction _10 = ilProcessor.Create(OpCodes.Call, callbackWithArg);
                //Instruction _11 = ilProcessor.Create(OpCodes.Nop);

                ilProcessor.InsertBefore(start, _01);
                ilProcessor.InsertAfter(_01, _02);
                ilProcessor.InsertAfter(_02, _03);
                ilProcessor.InsertAfter(_03, _04);
                ilProcessor.InsertAfter(_04, _05);

                ilProcessor.InsertAfter(_05, _06);
                ilProcessor.InsertAfter(_06, _07);

            */
        }
    }
}

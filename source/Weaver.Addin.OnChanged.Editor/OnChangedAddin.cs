using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using Weaver.Extensions;

namespace Weaver.Addin.OnChanged.Editor
{
    public class OnChangedAddin : WeaverAddin
    {
        public override string Name => "OnChanged Addin";

        public OnChangedAddin()
        {
        }

        public override void VisitModule(ModuleDefinition moduleDefinition)
        {
            base.VisitModule(moduleDefinition);
        }

        public override void VisitProperty(PropertyDefinition propertyDefinition)
        {
            CustomAttribute attribute = propertyDefinition.GetAttribute<OnChangedAttribute>();
            if (attribute == null)
            {
                return;
            }

            propertyDefinition.CustomAttributes.Remove(attribute);


            MethodReference getMethod = propertyDefinition.GetMethod;
            MethodReference setMethod = propertyDefinition.SetMethod;

            if (setMethod == null)
            {
                Logger.Warning(Name, $"{FormatError(propertyDefinition)} does not have setter. This attribute has no effect");
                return;
            }

            if (getMethod == null)
            {
                Logger.Error(Name, $"{FormatError(propertyDefinition)} does not have getter which is required. This attribute will have no effect");
                return;
            }

            if (attribute.HasConstructorArguments)
            {
                string callbackMethodName = (string)attribute.ConstructorArguments[0].Value;
                TypeReference propertyType = propertyDefinition.PropertyType;
                MethodReference callbackMethod = propertyDefinition.DeclaringType.GetMethod(callbackMethodName, propertyType);

                if (callbackMethod == null)
                {
                    Logger.Error(Name, $"{FormatError(propertyDefinition)} has the callback method set but not matching the signture of 'void {callbackMethodName}({propertyType.Name})` could be found'");
                    return;
                }

                InsertMethodCallback(setMethod.Resolve().Body.Instructions, getMethod, setMethod, callbackMethod);
            }
            else
            {
                // Use INotifyPropertyChanged
            }
        }

        private void InsertMethodCallback(Collection<Instruction> instructions, MethodReference getMethod, MethodReference setMethod, MethodReference callback)
            => instructions.InsertRange(0, new []
            {
                Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, getMethod),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Beq, instructions[0]),
                Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, callback),
                Instruction.Create(OpCodes.Nop)
            });


        private string FormatError(PropertyDefinition propertyDefinition)
            => $"The property {propertyDefinition.Name} defined in {propertyDefinition.DeclaringType.FullName}";
    }

}

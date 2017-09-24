using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Weaver.Development
{
    public class GUIDrawerComponent : WeaverComponent
    {
        public override string addinName
        {
            get { return "GUIDrawer"; }
        }

        public override DefinitionType effectedDefintions
        {
            get { return DefinitionType.Type | DefinitionType.Module; }
        }



        public override void VisitType(TypeDefinition typeDefinition)
        {
            const string ON_GUI_METHOD = "OnGUI";

            if(typeDefinition.GetMethod(ON_GUI_METHOD) != null)
            {
                // Don't edit anyone elses methods
                return;
            }

            TypeImplementation gameObjectTypeImp = new TypeImplementation(typeDefinition.Module, typeof(Object));
            MethodImplementation getGameObjectNameMethodImp = gameObjectTypeImp.GetProperty("name").Get();

            TypeImplementation guilayoutTypeImp = new TypeImplementation(typeDefinition.Module, typeof(GUILayout));
            MethodImplementation labelMethodImp = guilayoutTypeImp.GetMethod("Label", typeof(string), typeof(GUILayoutOption[]));

            TypeImplementation guiLayoutOptionTypeImp = new TypeImplementation(typeDefinition.Module, typeof(GUILayoutOption));

            // Create our method
            MethodDefinition onGuiMethodDefinition = new MethodDefinition(ON_GUI_METHOD, MethodAttributes.HideBySig | MethodAttributes.Public, typeSystem.Void);
            // Get the ILProcessor
            MethodBody body = onGuiMethodDefinition.Body;
            ILProcessor ilProcessor = body.GetILProcessor();
            // Write our instructions
            Instruction _00 = ilProcessor.Create(OpCodes.Nop);
            Instruction _01 = ilProcessor.Create(OpCodes.Ldarg_0);
            Instruction _02 = ilProcessor.Create(OpCodes.Call, getGameObjectNameMethodImp.reference);
            Instruction _03 = ilProcessor.Create(OpCodes.Ldc_I4_0);
            Instruction _04 = ilProcessor.Create(OpCodes.Newarr, guiLayoutOptionTypeImp.reference);
            Instruction _05 = ilProcessor.Create(OpCodes.Call, labelMethodImp.reference);
            Instruction _06 = ilProcessor.Create(OpCodes.Ret);

            body.Instructions.Add(_00);
            body.Instructions.Add(_01);
            body.Instructions.Add(_02);
            body.Instructions.Add(_03);
            body.Instructions.Add(_04);
            body.Instructions.Add(_05);
            body.Instructions.Add(_06);

            // Add the method to our type
            typeDefinition.Methods.Add(onGuiMethodDefinition);
        }
    }
}

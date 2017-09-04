using System;
using Mono.Cecil;
using UnityEngine;
using Mono.Cecil.Cil;
using System.Linq;
using System.Diagnostics;
using Mono.Collections.Generic;
using Debug = UnityEngine.Debug;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Addins/Method Timer", fileName = "Method Timer Addin")]
    public class MethodTimerAddin : WeaverPlugin
    {
        public struct StopwatchDefinition
        {
            public MethodReference consturctor;
            public MethodReference start;
            public MethodReference stop;
            public MethodReference getElapsedMilliseconds;

            public StopwatchDefinition(TypeDefinition stopwatchTypeDef, ModuleDefinition module)
            {
                consturctor = module.Import(stopwatchTypeDef.GetMethod(".ctor"));
                start = module.Import(stopwatchTypeDef.GetMethod("Start"));
                stop = module.Import(stopwatchTypeDef.GetMethod("Stop"));
                getElapsedMilliseconds = module.Import(stopwatchTypeDef.GetProperty("ElapsedMilliseconds").GetMethod);
            }
        }

        private TypeSystem m_TypeSystem;
        private StopwatchDefinition m_StopWatchTypeDef;
        private MethodReference m_StringConcatMethodRef;
        private MethodReference m_DebugLogMethodRef;
        private TypeReference m_StopwatchTypeReference;
        private bool m_IsVistingType;

        public override string addinName
        {
            get
            {
                return "Method Timer";
            }
        }


        public override DefinitionType effectedDefintions
        {
            get
            {
                return DefinitionType.Module | DefinitionType.Method | DefinitionType.Type;
            }
        }


        public void Initialize(ModuleDefinition moduleDefinition)
        {
            /*
            UnityEngine.Debug.Log("Initialize: " + moduleDefinition.Name);
            // Cache type system
            TypeSystem typeSystem = moduleDefinition.TypeSystem;

            // Create type
            TypeAttributes typeAttributes = TypeAttributes.BeforeFieldInit;
            TypeDefinition methodTimerTypeDefinition = new TypeDefinition("Weaver.Generated", "MethodTimer", typeAttributes, typeSystem.Object);
            moduleDefinition.Types.Add(methodTimerTypeDefinition);

            // Import Date Time
            TypeReference stopwatchTypeReference = moduleDefinition.Import(typeof(Stopwatch));

            // Create a StopWatch field
            FieldDefinition stopwatchField = new FieldDefinition("m_StopWatch", FieldAttributes.Private | FieldAttributes.Static, stopwatchTypeReference);
            methodTimerTypeDefinition.Fields.Add(stopwatchField);

            // StartTimer Method
            MethodDefinition StartMethod = new MethodDefinition("Start", MethodAttributes.Public | MethodAttributes.Static, typeSystem.Void);
            methodTimerTypeDefinition.Methods.Add(StartMethod);

            // StopTimer Method
            MethodDefinition StopMethod = new MethodDefinition("Stop", MethodAttributes.Public | MethodAttributes.Static, typeSystem.Void);
            methodTimerTypeDefinition.Methods.Add(StopMethod);

            TypeDefinition stopWatchTypeDefinition = stopwatchTypeReference.Resolve();
            MethodDefinition stopWatchStartMethod = stopWatchTypeDefinition.Methods.FirstOrDefault(x => x.Name == "Start");
            MethodBody startMethodBody = StartMethod.Body;

            var startMetho =  moduleDefinition.Import(stopWatchStartMethod);

            Instruction One = Instruction.Create(OpCodes.Ldsfld, stopwatchField);
            Instruction Two = Instruction.Create(OpCodes.Callvirt, startMetho);
            Instruction Three = Instruction.Create(OpCodes.Ret);
            startMethodBody.Instructions.Add(One);
            startMethodBody.Instructions.Add(Two);
            startMethodBody.Instructions.Add(Three);*/

        }

        public override void VisitType(TypeDefinition typeDefinition)
        {
            UnityEngine.Debug.Log(typeDefinition.Name);
            m_IsVistingType = typeDefinition.Name == "ExampleBehaviour";
        }

        public override void VisitModule(ModuleDefinition moduleDefinition)
        {
            // Get the type system
            m_TypeSystem = moduleDefinition.TypeSystem;
            // Import our stopwatch type reference 
            m_StopwatchTypeReference = moduleDefinition.Import(typeof(Stopwatch));
            // Resolve it so we can get the type definition
            TypeDefinition stopwatchTypeDef = m_StopwatchTypeReference.Resolve();
            // Create our value holder
            m_StopWatchTypeDef = new StopwatchDefinition(stopwatchTypeDef, moduleDefinition);
            // String
            TypeDefinition stringTypeDef = m_TypeSystem.String.Resolve();
            m_StringConcatMethodRef = moduleDefinition.Import(stringTypeDef.GetMethod("Concat", 2));

            TypeReference debugTypeRef = moduleDefinition.Import(typeof(UnityEngine.Debug));
            TypeDefinition debugTypeDeff = debugTypeRef.Resolve();
            m_DebugLogMethodRef = moduleDefinition.Import(debugTypeDeff.GetMethod("Log", 1));
        }

        public override void VisitMethod(MethodDefinition methodDefinition)
        {
            if (!m_IsVistingType)
            {
                return;
            }
            MethodBody body = methodDefinition.Body;
            Collection<Instruction> instructions = new Collection<Instruction>();
            ILProcessor bodyProcessor = body.GetILProcessor();

            VariableDefinition stopwatchVariable = new VariableDefinition("stopwatch", m_StopwatchTypeReference);
            VariableDefinition elapsedMilliseconds = new VariableDefinition("elapsedMilliseconds", m_TypeSystem.Int64);
            body.Variables.Add(stopwatchVariable);
            body.Variables.Add(elapsedMilliseconds);
            // Inject at the start of the function 
            {


                Instruction _00 = Instruction.Create(OpCodes.Newobj, m_StopWatchTypeDef.consturctor);
                Instruction _01 = Instruction.Create(OpCodes.Stloc, stopwatchVariable);
                Instruction _02 = Instruction.Create(OpCodes.Ldloc, stopwatchVariable);
                Instruction _03 = Instruction.Create(OpCodes.Callvirt, m_StopWatchTypeDef.start);

                bodyProcessor.InsertBefore(body.Instructions[0], _00);
                bodyProcessor.InsertAfter(_00, _01);
                bodyProcessor.InsertAfter(_01, _02);
                bodyProcessor.InsertAfter(_02, _03);
            }

            // [Normal part of function]

            // Inject at the end
            {


                // Calls a late - bound method on an object, pushing the return value onto the evaluation stack
                Instruction _00 = Instruction.Create(OpCodes.Ldloc, stopwatchVariable);
                // Calls a late - bound method on an object, pushing the return value onto the evaluation stack
                Instruction _01 = Instruction.Create(OpCodes.Callvirt, m_StopWatchTypeDef.stop);
                // Pushes the integer value of 0 onto the evaluation stack as an int32.
                Instruction _02 = Instruction.Create(OpCodes.Ldc_I4_0);
                // Converts the value on top of the evaluation stack to int64.
                Instruction _03 = Instruction.Create(OpCodes.Conv_I8);
                // Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 1.
                Instruction _04 = Instruction.Create(OpCodes.Stloc, elapsedMilliseconds);
                // Loads the local variable at index 0 onto the evaluation stack.
                Instruction _05 = Instruction.Create(OpCodes.Ldloc, stopwatchVariable);
                // Calls a late - bound method on an object, pushing the return value onto the evaluation stack. Using the get method
                Instruction _06 = Instruction.Create(OpCodes.Callvirt, m_StopWatchTypeDef.getElapsedMilliseconds);
                // Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 1.
                Instruction _07 = Instruction.Create(OpCodes.Stloc, elapsedMilliseconds);
                // Pushes a new object reference to a string literal stored in the metadata.
                Instruction _08 = Instruction.Create(OpCodes.Ldstr, methodDefinition.Name);
                // Loads the local variable at index 1 onto the evaluation stack.
                Instruction _09 = Instruction.Create(OpCodes.Ldloc, elapsedMilliseconds);
                // Converts a value type to an object reference (type O).
                Instruction _10 = Instruction.Create(OpCodes.Box, m_TypeSystem.Int64);
                // Calls the method indicated by the passed method descriptor.
                Instruction _11 = Instruction.Create(OpCodes.Call, m_StringConcatMethodRef);
                // Calls the method indicated by the passed method descriptor.
                Instruction _12 = Instruction.Create(OpCodes.Call, m_DebugLogMethodRef);
                // Returns from the current method, pushing a return value (if present) from the callee's evaluation stack onto the caller's evaluation stack.
                Instruction _13 = Instruction.Create(OpCodes.Ret);

                bodyProcessor.InsertBefore(body.Instructions[body.Instructions.Count - 1], _00);
                bodyProcessor.InsertAfter(_00, _01);
                bodyProcessor.InsertAfter(_01, _02);
                bodyProcessor.InsertAfter(_02, _03);
                bodyProcessor.InsertAfter(_03, _04);
                bodyProcessor.InsertAfter(_04, _05);
                bodyProcessor.InsertAfter(_05, _06);
                bodyProcessor.InsertAfter(_06, _07);
                bodyProcessor.InsertAfter(_07, _08);
                bodyProcessor.InsertAfter(_08, _09);
                bodyProcessor.InsertAfter(_09, _10);
                bodyProcessor.InsertAfter(_10, _11);
                bodyProcessor.InsertAfter(_11, _12);
            }
        }
    }
}

using System;
using Mono.Cecil;
using UnityEngine;
using Mono.Cecil.Cil;
using System.Linq;
using System.Diagnostics;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Addins/Method Timer", fileName = "Method Timer Addin")]
    public class MethodTimerAddin : WeaverPlugin
    {
        public override string addinName
        {
            get
            {
                return "Method Timer";
            }
        }

        public override Type[] GetAttributes()
        {
            return new Type[] { typeof(MethodTimerAttribute) };
        }

        public override void Initialize(ModuleDefinition moduleDefinition)
        {
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
            startMethodBody.Instructions.Add(Three);
        }

        public override void OnMatch(Type attributeType, MemberReference effectedMemeber)
        {
            throw new NotImplementedException();
        }
    }
}

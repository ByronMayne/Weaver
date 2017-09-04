using System;
using Mono.Cecil;
using UnityEngine;
using Mono.Cecil.Cil;
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
            TypeAttributes typeAttributes = TypeAttributes.BeforeFieldInit | TypeAttributes.AnsiClass | TypeAttributes.AutoClass;
            TypeDefinition stopwatchType = new TypeDefinition("Weaver", "MethodTimer", typeAttributes, typeSystem.Object);
            moduleDefinition.Types.Add(stopwatchType);
        }

        public override void OnMatch(Type attributeType, MemberReference effectedMemeber)
        {
            throw new NotImplementedException();
        }
    }
}

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using Weaver.Contracts;
using Weaver.Contracts.Diagnostics;
using Weaver.Core;
using Weaver.Extensions;

namespace Weaver.Addin.MethodTimer.Editor
{
    public class MethodTimerAddin : IWeaverAddin
    {
        public string Name => "Method Timer";
        private StopwatchDefinition m_stopwatchDefinition;
        private StringDefinition m_stringDefinition;
        private TypeSystem m_typeSystem;

        public ILogger Logger { get; set; }

        public DefinitionType AffectedDefinitions => DefinitionType.Modules | DefinitionType.Method;

        public void AfterWeave()
        {

        }

        public void BeforeWeave()
        {
        }


        public void Visit(ModuleDefinition moduleDefinition)
        {
            m_stopwatchDefinition = new StopwatchDefinition(moduleDefinition);
            m_stringDefinition = new StringDefinition(moduleDefinition);
            m_typeSystem = moduleDefinition.TypeSystem;
        }

        public void Visit(MethodDefinition methodDefinition)
        {
            CustomAttribute attribute = methodDefinition.GetAttribute<MethodTimerAttribute>();
            if (attribute == null)
            {
                return;
            }

            methodDefinition.CustomAttributes.Remove(attribute);


            MethodBody body = methodDefinition.Body;
            Collection<Instruction> instructions = body.Instructions;

            VariableDefinition stopwatchVariable = new VariableDefinition(m_stopwatchDefinition.Type);
            VariableDefinition elapsedMilliseconds = new VariableDefinition(m_typeSystem.Int64);

            body.Variables.Add(stopwatchVariable);
            body.Variables.Add(elapsedMilliseconds);

            InjectStart(instructions, stopwatchVariable);


            bool foundReturn = false;
            for (int i = body.Instructions.Count - 1; i >= 0; i--)
            {
                Instruction instruction = body.Instructions[i];

                if(instruction.OpCode == OpCodes.Ret)
                {
                    foundReturn = true;
                }

                if(instruction.OpCode == OpCodes.Nop && foundReturn)
                {
                    foundReturn = false;
                    // Insert Timer
                }

            }
        }

        /// <summary>
        /// Injects the timer into our method and invokes start on it. 
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="stopwatch">The stopwatch.</param>
        private void InjectStart(Collection<Instruction> instructions, VariableDefinition stopwatch)
            => instructions.InsertRange(0, new[] {
                 Instruction.Create(OpCodes.Newobj, m_stopwatchDefinition.Consturctor),
                 Instruction.Create(OpCodes.Stloc, stopwatch),
                 Instruction.Create(OpCodes.Ldloc, stopwatch),
                 Instruction.Create(OpCodes.Callvirt, m_stopwatchDefinition.Start)});


        public void Visit(AssemblyDefinition assemblyDefinition)
        { }

        public void Visit(TypeDefinition typeDefinition)
        { }

        public void Visit(PropertyDefinition propertyDefinition)
        { }

        public void Visit(FieldDefinition fieldDefinition)
        { }

        public void Visit(EventDefinition eventDefinition)
        { }
    }
}

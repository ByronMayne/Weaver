using Mono.Cecil;
using System.Diagnostics;
using Weaver;

namespace Weaver.Addin.MethodTimer.Editor
{
    internal class StopwatchDefinition
    {
        public readonly TypeDefinition Type;
        public readonly MethodDefinition Consturctor;
        public readonly MethodDefinition Start;
        public readonly MethodDefinition Stop;
        public readonly MethodDefinition ElapsedMilliseconds;

        public StopwatchDefinition(ModuleDefinition moduleDefinition)
        {
            TypeReference stopwatchTypeRef = moduleDefinition.ImportReference(typeof(Stopwatch));
            TypeDefinition stopwatchTypeDef = stopwatchTypeRef.Resolve();

            moduleDefinition.Import<Stopwatch>()
                .GetType(out Type)
                .GetConstructor(out Consturctor)
                .GetMethod(s => s.Start, out Start)
                .GetMethod(s => s.Stop, out Stop)
                .GetProperty(t => t.ElapsedMilliseconds)
                    .GetGetter(out ElapsedMilliseconds);

        }
    }
}

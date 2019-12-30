using Mono.Cecil;
using System.Diagnostics;

namespace Weaver.Addin.MethodTimer.Editor
{
    internal class ILStopwatch
    {
        public readonly TypeDefinition Definition;
        public readonly TypeReference Reference;
        public readonly MethodReference Consturctor;
        public readonly MethodReference Start;
        public readonly MethodReference Stop;
        public readonly MethodReference Elapsed;

        public ILStopwatch(ModuleDefinition moduleDefinition)
        {
            moduleDefinition.ImportFluent<Stopwatch>()
                .GetType(out Definition)
                .GetType(out Reference)
                .GetConstructor(out Consturctor)
                .GetMethod(s => s.Start, out Start)
                .GetMethod(s => s.Stop, out Stop)
                .GetProperty(s => s.Elapsed)
                    .GetGetter(out Elapsed);
        }
    }
}

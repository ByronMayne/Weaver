using Mono.Cecil;

namespace Weaver.Addin.MethodTimer.Editor
{
    class StringDefinition
    {
        public readonly MethodDefinition Concat;
        public readonly TypeDefinition Definition;

        public StringDefinition(ModuleDefinition moduleDefinition)
        {
            moduleDefinition.ImportFluent<string>()
                  .GetType(out Definition)
                  .GetStaticMethod(() => string.Concat(string.Empty, string.Empty), out Concat);
        }
    }
}

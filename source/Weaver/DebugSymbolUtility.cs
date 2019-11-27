using Seed.IO;
using System.IO;
using Weaver.Core;

namespace Weaver
{
    public static class DebugSymbolUtility
    {
        /// <summary>
        /// Gets the type of the symbols for the assembly at the given path.
        /// </summary>
        /// <param name="assemblyPath">The absolute path to the request assembly.</param>
        public static DebugSymbolType GetFromAssemblyPath(AbsolutePath assemblyPath)
        {
            AbsolutePath pdbPath = (AbsolutePath)Path.ChangeExtension(assemblyPath, ".pdb");
            AbsolutePath mdbPath = (AbsolutePath)Path.ChangeExtension(assemblyPath, ".dll.mdb");

            if (File.Exists(pdbPath))
                return DebugSymbolType.Program;

            if (File.Exists(mdbPath))
                return DebugSymbolType.Mono;

            return DebugSymbolType.Unknown;
        }
    }
}

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;

namespace Weaver
{
    public class WeaverAssemblyResolver : DefaultAssemblyResolver
    {
        public WeaverAssemblyResolver(string assemblyPath)
        {
            var asm = UnityEditor.Compilation.CompilationPipeline.GetAssemblies().FirstOrDefault(x => x.outputPath == assemblyPath);
            List<string> dependencies = new()
            {
                UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath(),
                Path.GetDirectoryName(asm.outputPath)
            };
            foreach (string refer in asm.compiledAssemblyReferences)
            {
                var directory = Path.GetDirectoryName(refer);
                if (dependencies.Contains(directory) == false)
                    dependencies.Add(directory);
            }
            if (dependencies != null)
            {
                foreach (var str in dependencies)
                {
                    AddSearchDirectory(str);
                }
            }
            AddSearchDirectory(assemblyPath);
            AddSearchDirectory(Path.GetDirectoryName(EditorApplication.applicationPath) + "\\Data\\Managed");
        }


    }
}
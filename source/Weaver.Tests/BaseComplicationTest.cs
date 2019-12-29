using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weaver.Contracts;
using Weaver.Core;

namespace Weaver.Tests
{
    public abstract class BaseComplicationTest
    {
        /// <summary>
        /// Appends the references that go into our coplication unit 
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        protected virtual void AppendReferences(ISet<string> assemblyPaths)
        {
            assemblyPaths.Add(typeof(WeaverAddin).Assembly.Location); // Weaver
            assemblyPaths.Add(typeof(IWeaverAddin).Assembly.Location); // Weaver.Contracts
            assemblyPaths.Add(typeof(DefinitionType).Assembly.Location); // Weaver.Core
            assemblyPaths.Add(typeof(AbsolutePath).Assembly.Location); // Seed.IO
        }

        /// <summary>
        /// Compiles the specified source code and returns back the path to the assembly.
        /// </summary>
        /// <param name="sourceCode">The source code.</param>
        protected string Compile(string sourceCode)
        {
            string fileName = Path.GetRandomFileName();
            string outputPath = $"{Path.GetTempPath()}{fileName}.dll";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, CSharpParseOptions.Default);
            ISet<string> assemblies = new HashSet<string>();
            AppendReferences(assemblies);

            CSharpCompilationOptions options 
                = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug, deterministic: true);

            CSharpCompilation compilation = CSharpCompilation.Create($"Assembly-{fileName}",  new[] { syntaxTree }, assemblies.Select(a => MetadataReference.CreateFromFile(a)), options);
            EmitResult result = compilation.Emit(outputPath);

            if(!result.Success)
            {
                string[] messages = result.Diagnostics.Select(d => d.GetMessage()).ToArray();
                Assert.Fail($"Filed to compile assembly because of the following \n{string.Join("\n", messages)}");
            }
            Console.WriteLine($"Output: {outputPath}");
            return outputPath;
        }
    }
}

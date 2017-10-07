using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Text;

public abstract class BaseWeaverTest
{
    /// <summary>
    /// Used to populate the list of types from each assembly.
    /// </summary>
    public virtual void PopulateAssemblies(IList<Type> assemblyTypes)
    {
        assemblyTypes.Add(typeof(System.Object));
        assemblyTypes.Add(typeof(UnityEngine.Object));
        assemblyTypes.Add(typeof(UnityEditor.Editor));
        assemblyTypes.Add(typeof(Weaver.WeaverSettings));
        assemblyTypes.Add(typeof(Weaver.OnChangedAttribute));
    }

    /// <summary>
    /// Compiles a set of source files for a test. This handles failing the test if it
    /// does not compile
    /// </summary>
    public CompilerResults CompileTest(params string[] sourceFiles)
    {
        CSharpCodeProvider compiler = new CSharpCodeProvider();
        CompilerParameters paramaters = new CompilerParameters()
        {
            GenerateInMemory = false,
            GenerateExecutable = false,
            MainClass = string.Empty
        };
        List<Type> assemblyTypes = new List<Type>();
        PopulateAssemblies(assemblyTypes);
        for (int i = 0; i < assemblyTypes.Count; i++)
        {
            // Get the types assembly
            Assembly assembly = assemblyTypes[i].Assembly;
            // Get its location on disk
            string assemblyPath = assembly.Location;
            // Add it as a reference 
            paramaters.ReferencedAssemblies.Add(assemblyPath);
        }
        // Compile the code
        CompilerResults results = compiler.CompileAssemblyFromFile(paramaters, sourceFiles);
        // Check for errors and if we have any fail the test
        if (results.Errors.Count != 0)
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine("Unable to compile the test assembly.");
            logBuilder.AppendLine("=== Source Files ===");
            for(int i= 0; i < sourceFiles.Length; i++)
            {
                logBuilder.AppendLine(string.Format("{0}: {1}", i + 1, sourceFiles[i]));
            }

            logBuilder.AppendLine("=== Errors ===");
            for (int i = 0; i < results.Errors.Count; i++)
            {
                logBuilder.AppendLine(results.Errors[i].ToString());
            }
            Assert.Fail(logBuilder.ToString());
        }
        return results;
    }
}

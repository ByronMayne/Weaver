using System.Collections.Generic;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Text;
using Weaver;
using Mono.Collections.Generic;
using Mono.Cecil;
using System.IO;

public abstract class BaseWeaverTest
{
    public struct AssemblyTestResult
    {
        public Assembly baseAssembly;
        public Assembly weavedAssembly; 
    }

    private WeaverSettings m_Settings;
    private AppDomain m_TestDomain; 

    [SetUp]
    public virtual void Setup()
    {
        string domainName = "Weaver.Tests." + GetType().FullName;
        // Create a domain for our test
        Log("Setup", "Creating App Domain " + domainName); 
        m_TestDomain = AppDomain.CreateDomain(domainName);
        // Try to get our instance
        Log("Setup", "Gettings Settings Instnace" + domainName);
        m_Settings = WeaverSettings.Instance();
        // Assume it's not null otherwise our tests can't be run
        Assume.That(m_Settings != null, "We can't run tests if we don't have a Weaver Settings instance in the project.");
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Cleanup our testing domain
        Log("Teardown", "Unloading App Domain" + m_TestDomain.FriendlyName);
        AppDomain.Unload(m_TestDomain);
    }



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
    public AssemblyTestResult CompileTest(params string[] sourceFiles)
    {
        Log("Compiling", "Creating Compiler");
        CSharpCodeProvider compiler = new CSharpCodeProvider();
        CompilerParameters paramaters = new CompilerParameters()
        {
            GenerateInMemory = false,
            GenerateExecutable = false,
            IncludeDebugInformation = true,
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
            Log("Compiling", "Added Assembly Refernece " + assembly.FullName);
        }
        // Compile the code
        CompilerResults results = compiler.CompileAssemblyFromFile(paramaters, sourceFiles);
        // Check for errors and if we have any fail the test
        if (results.Errors.Count != 0)
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine("Unable to compile the test assembly.");
            logBuilder.AppendLine("=== Source Files ===");
            for (int i = 0; i < sourceFiles.Length; i++)
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
        // Read the module
        Log("Cecil", "Reading Module " + results.PathToAssembly);
        ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(results.PathToAssembly);
        // Create a collection to visit
        Collection<ModuleDefinition> editedModules = new Collection<ModuleDefinition>() { moduleDefinition };
        // Invoke the visite
        Log("Compiling", "Visiting Modules");
        m_Settings.componentController.VisitModules(editedModules);
        // Save it back to disk
        string modifiedModulePath = results.PathToAssembly + "__weaved";
        // Save the modified one to disk
        Log("Compiling", "Writing Weaved Assembly to disk at " + modifiedModulePath);
        moduleDefinition.Write(modifiedModulePath);

        // Create our result
        AssemblyTestResult result = new AssemblyTestResult();
        result.baseAssembly = m_TestDomain.Load(results.PathToAssembly); 
        result.weavedAssembly = m_TestDomain.Load(modifiedModulePath);

        // Return the result.
        return result;
    }

    /// <summary>
    /// Logs a message to the console with some text. This is just a short hand around
    /// <see cref="Console.WriteLine"/>
    /// </summary>
    protected void Log(string tag, string message)
    {
        Console.WriteLine(string.Format("[{0}]: {1}", tag, message));
    }
}

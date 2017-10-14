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
    /// <summary>
    /// The delegate used to provide source code for our test.
    /// </summary>
    /// <returns></returns>
    public delegate string SourceProviderDelegate();

    public struct AssemblyTestResult
    {
        public Assembly baseAssembly;
        public Assembly weavedAssembly;
    }

    private WeaverSettings m_Settings;

    [SetUp]
    public virtual void Setup()
    {
        // Try to get our instance
        Log("Setup", "Gettings Settings Instance");
        m_Settings = WeaverSettings.Instance();
        // Assume it's not null otherwise our tests can't be run
        Assume.That(m_Settings != null, "We can't run tests if we don't have a Weaver Settings instance in the project.");
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Cleanup our testing domain
        Log("Teardown", "Test Complete");
    }
    
    /// <summary>
    /// Used to populate the list of types from each assembly.
    /// </summary>
    public virtual void PopulateAssemblies(IList<Type> assemblyTypes)
    {
        //assemblyTypes.Add(typeof(System.Object));
        //assemblyTypes.Add(typeof(UnityEngine.Object));
        //assemblyTypes.Add(typeof(UnityEditor.Editor));
        //assemblyTypes.Add(typeof(Weaver.OnChangedAttribute));
        //assemblyTypes.Add(typeof(Weaver.WeaverSettings));
    }

    /// <summary>
    /// Compiles a set of source files for a test. This handles failing the test if it
    /// does not compile
    /// </summary>
    public AssemblyTestResult CompileTest(SourceProviderDelegate sourceProvider)
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

        // Invoke our soure provider
        string sourceCode = sourceProvider();

        // Check if we have source
        if(string.IsNullOrEmpty(sourceCode))
        {
            throw new InvalidOperationException("The source provider function did not return any source to compile");
        }

        CompilerResults results = compiler.CompileAssemblyFromSource(paramaters, sourceCode);
        // Check for errors and if we have any fail the test
        if (results.Errors.Count != 0)
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine("Unable to compile the test assembly.");
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
        m_Settings.componentController.VisitModules(editedModules, null);
        // Save it back to disk
        string modifiedModulePath = results.PathToAssembly;
        int extensionIndex = modifiedModulePath.LastIndexOf('.');
        modifiedModulePath = modifiedModulePath.Insert(extensionIndex, "__weaved");
        // Set it's assembly name
        AssemblyNameDefinition assemblyName = moduleDefinition.Assembly.Name;
        assemblyName.Name += "__weaved";
        // Save the modified one to disk
        Log("Compiling", "Writing Weaved Assembly to disk at " + modifiedModulePath);
        WriterParameters writerParameters = new WriterParameters();
        writerParameters.WriteSymbols = true;
        writerParameters.SymbolWriterProvider = new Mono.Cecil.Mdb.MdbWriterProvider();
        moduleDefinition.Runtime = TargetRuntime.Net_2_0;
        moduleDefinition.Write(modifiedModulePath, writerParameters);

        Log("Loading", "Loading Assemblies into Test Domain");
        AssemblyTestResult result = new AssemblyTestResult();
        result.baseAssembly = results.CompiledAssembly;
        Log("Assembly", Assembly.GetCallingAssembly().FullName);

        result.weavedAssembly = AppDomain.CurrentDomain.Load(modifiedModulePath);
        Log("Loading", "Return result for test");
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

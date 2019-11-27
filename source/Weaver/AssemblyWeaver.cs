using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Seed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Weaver.Contracts;
using Weaver.Contracts.Diagnostics;
using Weaver.Core;

namespace Weaver
{
    /// <summary>
    /// The default implemention for the IAssemblyWeaver interface. It's the entry point to the whole thing.
    /// </summary>
    /// <seealso cref="Weaver.Contracts.IAssemblyWeaver" />
    public class AssemblyWeaver : IAssemblyWeaver
    {
        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        public IAssemblyResolver AssemblyResolver { get; }

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the assembly cache.
        /// </summary>
        public IAssemblyCache AssemblyCache { get; }

        /// <summary>
        /// Gets or sets the logger that we output too
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyWeaver"/> class.
        /// </summary>
        public AssemblyWeaver() : this(new DefaultAssemblyResolver())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyWeaver"/> class.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver to use.</param>
        public AssemblyWeaver(IAssemblyResolver assemblyResolver)
        {
            WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            AssemblyResolver = assemblyResolver;
            AssemblyCache = new AssemblyCache(assemblyResolver);
            Logger = Diagnostics.Logger.Default;
        }

        /// <summary>
        /// Weaves an assembly from disk.
        /// </summary>
        /// <param name="assemblyPath">The system path to the assembly.</param>
        /// <param name="outputLog">The output log.</param>
        /// <param name="addIns">The add ins youu would like to run.</param>
        /// <returns>
        /// True if it's successful and false if it's not.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool WeaveAssembly(string assemblyPath, IEnumerable<IWeaverAddin> addIns)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentNullException(nameof(assemblyPath));

            if (addIns == null)
                throw new ArgumentNullException(nameof(addIns));

            if (!addIns.Any())
            {
                Logger.Warning(nameof(AssemblyWeaver), "No add-ins were provided, weaving will be skipped since there is nothing to do");
                return true;
            }

            AbsolutePath assemblyLocation;

            if (PathUtility.HasRoot(assemblyPath))
            {
                assemblyLocation = new AbsolutePath(assemblyPath);
            }
            else
            {
                assemblyLocation = new AbsolutePath(WorkingDirectory);
                assemblyLocation /= assemblyPath;
            }

            if (!File.Exists(assemblyLocation))
            {
                throw new FileNotFoundException($"Unable to find the assembly {Path.GetFileName(assemblyPath)} at path '{assemblyPath}'.");
            }

            AssemblyDefinition assemblyDefinition = AssemblyCache.Get(assemblyLocation);

            try
            {
                Logger.Info(nameof(AssemblyWeaver), "WeaveAssembly");
                Logger.Info(nameof(AssemblyWeaver), $"Path: {assemblyPath}");
                Logger.Info(nameof(AssemblyWeaver), $"Addins:");

                IWeaverAddin[] addinArray = addIns.ToArray();

                foreach (IWeaverAddin addin in addinArray)
                {
                    Logger.Info(nameof(AssemblyWeaver), $" - {addin.Name}");
                }

                Logger.Info(nameof(AssemblyWeaver), $"Visiting");
                Visit(assemblyDefinition, addinArray);

                WriterParameters writerParameters = GetWriterParameters(assemblyLocation);

                Logger.Info(nameof(AssemblyWeaver), $"Writing");
                assemblyDefinition.Write(writerParameters);
            }
            catch (Exception e)
            {
                Logger.Exception(nameof(AssemblyWeaver), e);
                return false;
            }

            Logger.Info(nameof(AssemblyWeaver), $"Successful");
            return true;
        }

        /// <summary>
        /// Gets the writer parameters for writing assemblies to disk.
        /// </summary>
        private static WriterParameters GetWriterParameters(AbsolutePath assemblyPath)
        {
            ISymbolWriterProvider symbolWriterProvider = null; 
            DebugSymbolType symbolType = DebugSymbolUtility.GetFromAssemblyPath(assemblyPath);
            switch (symbolType)
            {
                case DebugSymbolType.Mono:
                    symbolWriterProvider = new MdbWriterProvider();
                    break;
                case DebugSymbolType.Program:
                    symbolWriterProvider = new PortablePdbWriterProvider();
                    break; 
            }

            WriterParameters writeParams = new WriterParameters()
            {
                WriteSymbols = symbolType != DebugSymbolType.Unknown,
                SymbolWriterProvider = symbolWriterProvider,
            };

            return writeParams;
        }

        /// <summary>
        /// Weaves an assembly from disk.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="outputLog">The output log.</param>
        /// <param name="addIns">The add ins youu would like to run. They must from <see cref="IWeaverAddin"/> and have a zero argument constructor.</param>
        /// <returns>
        /// True if it's successful and false if it's not.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool WeaveAssembly(string assemblyPath, ILogger outputLog, IEnumerable<Type> addIns)
        {
            IList<IWeaverAddin> createdAddins = new List<IWeaverAddin>();

            foreach (Type type in addIns)
            {
                if (!typeof(IWeaverAddin).IsAssignableFrom(type))
                {
                    Exception exception = new ArgumentException($"The type {type.FullName} does not inherit from the required type {typeof(IWeaverAddin)}");
                    outputLog.Exception(nameof(AssemblyWeaver), exception);
                }

                IWeaverAddin instance = (IWeaverAddin)Activator.CreateInstance(type, true);
                createdAddins.Add(instance);
            }

            return WeaveAssembly(assemblyPath, outputLog, addIns);
        }

        /// <summary>
        /// Visits the specified assembly definition.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition.</param>
        /// <param name="addins">The addins.</param>
        private void Visit(AssemblyDefinition assemblyDefinition, IReadOnlyCollection<IWeaverAddin> addins)
        {
            ForEach(addins, e => e.Visit(assemblyDefinition));
            foreach (ModuleDefinition moduleDefinition in assemblyDefinition.Modules)
            {
                Visit(moduleDefinition, addins);
            }
        }

        /// <summary>
        /// Visits the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="addins">The addins.</param>
        private void Visit(ModuleDefinition moduleDefinition, IReadOnlyCollection<IWeaverAddin> addins)
        {
            ForEach(addins, e => e.Visit(moduleDefinition));
            foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                Visit(typeDefinition, addins);
            }
        }

        /// <summary>
        /// Visits the specified type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="addins">The addins.</param>
        private void Visit(TypeDefinition typeDefinition, IReadOnlyCollection<IWeaverAddin> addins)
        {
            ForEach(addins, e => e.Visit(typeDefinition));

            if (typeDefinition.HasNestedTypes)
            {
                foreach (TypeDefinition nestTypeDefinition in typeDefinition.NestedTypes)
                {
                    Visit(typeDefinition, addins);
                }
            }

            foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
            {
                ForEach(addins, e => e.Visit(methodDefinition));
            }

            foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
            {
                ForEach(addins, e => e.Visit(propertyDefinition));
            }

            foreach (FieldDefinition fieldDefinition in typeDefinition.Fields)
            {
                ForEach(addins, e => e.Visit(fieldDefinition));
            }

            foreach (EventDefinition eventDefinition in typeDefinition.Events)
            {
                ForEach(addins, e => e.Visit(eventDefinition));
            }
        }

        // Invokes an action on all of our addins 
        private static void ForEach(IEnumerable<IWeaverAddin> targets, Action<IWeaverAddin> action)
        {
            foreach (IWeaverAddin addin in targets)
            {
                action(addin);
            }
        }

        public bool WeaveAssembly(string assemblyPath, ILogger outputLog, IEnumerable<IWeaverAddin> addIns)
        {
            throw new NotImplementedException();
        }
    }
}

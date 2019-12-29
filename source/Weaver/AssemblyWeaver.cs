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
        public AssemblyWeaver()
        {
            WorkingDirectory = Environment.CurrentDirectory;
            AssemblyCache = new AssemblyCache();
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

            try
            {
                Logger.Info(nameof(AssemblyWeaver), "WeaveAssembly");
                Logger.Info(nameof(AssemblyWeaver), $"Path: {assemblyPath}");

                Logger.Info(nameof(AssemblyWeaver), $"Starting");

                // Make a copy so that we can edit it.
                Start(assemblyLocation, new List<IWeaverAddin>(addIns));
            }
            catch (Exception e)
            {
                Logger.Error(nameof(AssemblyWeaver), $"Exception was thrown while weaving no changes will be applied to the assembly.");

                // Clear it from the cache as it could be invalid now
                AssemblyCache.Remove(assemblyLocation);

                Logger.Exception(nameof(AssemblyWeaver), e);
                return false;
            }

            Logger.Info(nameof(AssemblyWeaver), $"Successful");
            return true;
        }

        /// <summary>
        /// Starts the weaving process for an assembly. If one of our addins throws an exception it will be removed and we 
        /// will retry again. 
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="affectedDefintions">The affected defintions.</param>
        /// <param name="addins">The addins we are going to be running.</param>
        private void Start(AbsolutePath assemblyLocation, List<IWeaverAddin> addins)
        {
            DefinitionType affectedDefintions = DefinitionType.None;

            try
            {
                Logger.Info(nameof(AssemblyWeaver), $"Addins:");

                foreach (IWeaverAddin addin in addins)
                {
                    affectedDefintions |= addin.AffectedDefinitions;
                    Logger.Info(nameof(AssemblyWeaver), $" - {addin.Name}");
                }

                AssemblyDefinition assemblyDefinition = AssemblyCache.Get(assemblyLocation, false);

                Visit(assemblyDefinition, affectedDefintions, addins);

                WriterParameters writerParameters = GetWriterParameters(assemblyLocation);

                Logger.Info(nameof(AssemblyWeaver), $"Writing");

                assemblyDefinition.Write(writerParameters);
            }
            catch (AddinException addinException)
            {
                if (addins.Count > 1)
                {
                    Logger.Error(nameof(AssemblyWeaver), $"An expection was thrown by {addinException.Context.Name} while weaving. The addin will be removed and we will retry again");

                    // Just remove the add in so we can run the ones that work.
                    addins.Remove(addinException.Context);

                    // It's in a broken state, it must be removed. 
                    AssemblyCache.Remove(assemblyLocation);

                    Logger.Info(nameof(AssemblyWeaver), "Retrying");

                    // Run it again
                    Start(assemblyLocation, addins);

                    // We are done here.
                    return; 
                }
                else
                {
                    Logger.Error(nameof(AssemblyWeaver), $"No addins run succssful so no modifications were made to the assembly");
                }

                Logger.Exception(nameof(AssemblyWeaver), addinException);
            }
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
        public bool WeaveAssembly(string assemblyPath, IEnumerable<Type> addIns)
        {
            IList<IWeaverAddin> createdAddins = new List<IWeaverAddin>();

            foreach (Type type in addIns)
            {
                if (!typeof(IWeaverAddin).IsAssignableFrom(type))
                {
                    Exception exception = new ArgumentException($"The type {type.FullName} does not inherit from the required type {typeof(IWeaverAddin)}");
                    Logger.Exception(nameof(AssemblyWeaver), exception);
                }

                IWeaverAddin instance = (IWeaverAddin)Activator.CreateInstance(type, true);
                createdAddins.Add(instance);
            }

            return WeaveAssembly(assemblyPath, createdAddins);
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
        /// Visits the specified assembly definition.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition.</param>
        /// <param name="affectedDefintions">The types of definitions all addins affect</param>
        /// <param name="addins">The addins.</param>
        private void Visit(AssemblyDefinition assemblyDefinition, DefinitionType affectedDefintions, IReadOnlyCollection<IWeaverAddin> addins)
        {
            if (affectedDefintions == DefinitionType.None)
            {
                Logger.Warning(nameof(AssemblyWeaver), "No Add Ins had any affected definitions defined. " +
                    $"You most likely missed setting the filed {nameof(IWeaverAddin)}.{nameof(IWeaverAddin.AffectedDefinitions)}");
                return;
            }

            VisitAddIn(addins, e => e.VisitAssembly(assemblyDefinition), DefinitionType.Assembly);
            foreach (ModuleDefinition moduleDefinition in assemblyDefinition.Modules)
            {
                Visit(moduleDefinition, affectedDefintions, addins);
            }
        }

        /// <summary>
        /// Visits the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="addins">The addins.</param>
        private void Visit(ModuleDefinition moduleDefinition, DefinitionType affectedDefintions, IReadOnlyCollection<IWeaverAddin> addins)
        {
            VisitAddIn(addins, e => e.VisitModule(moduleDefinition), DefinitionType.Modules);
            foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                Visit(typeDefinition, affectedDefintions, addins);
            }
        }

        /// <summary>
        /// Visits the specified type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="addins">The addins.</param>
        private void Visit(TypeDefinition typeDefinition, DefinitionType affectedDefintions, IReadOnlyCollection<IWeaverAddin> addins)
        {
            VisitAddIn(addins, e => e.VisitType(typeDefinition), DefinitionType.Type);

            if (typeDefinition.HasNestedTypes)
            {
                foreach (TypeDefinition nestTypeDefinition in typeDefinition.NestedTypes)
                {
                    Visit(typeDefinition, affectedDefintions, addins);
                }
            }

            if ((affectedDefintions & DefinitionType.Method) != 0)
            {
                foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    VisitAddIn(addins, e => e.VisitMethod(methodDefinition), DefinitionType.Method);
                }
            }

            if ((affectedDefintions & DefinitionType.Property) != 0)
            {
                foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
                {
                    VisitAddIn(addins, e => e.VisitProperty(propertyDefinition), DefinitionType.Property);
                }
            }

            if ((affectedDefintions & DefinitionType.Field) != 0)
            {
                foreach (FieldDefinition fieldDefinition in typeDefinition.Fields)
                {
                    VisitAddIn(addins, e => e.VisitField(fieldDefinition), DefinitionType.Field);
                }
            }

            if ((affectedDefintions & DefinitionType.Event) != 0)
            {
                foreach (EventDefinition eventDefinition in typeDefinition.Events)
                {
                    VisitAddIn(addins, e => e.VisitEvent(eventDefinition), DefinitionType.Event);
                }
            }
        }

        // Invokes an action on all of our addins 
        private static void VisitAddIn(IEnumerable<IWeaverAddin> targets, Action<IWeaverAddin> action, DefinitionType definitionType)
        {
            foreach (IWeaverAddin addin in targets)
            {
                if ((addin.AffectedDefinitions & definitionType) != 0)
                {
                    try
                    {
                        action(addin);
                    }
                    catch (Exception e)
                    {
                        AddinException addinException = new AddinException(addin, e);
                        throw addinException;
                    }
                }
            }
        }
    }
}

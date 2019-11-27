using Seed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Weaver.Contracts;
using Weaver.Contracts.Diagnostics;
using Weaver.Diagnostics;

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
        /// Initializes a new instance of the <see cref="AssemblyWeaver"/> class.
        /// </summary>
        public AssemblyWeaver()
        {
            WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
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
        public bool WeaveAssembly(string assemblyPath, ILogger outputLog, IEnumerable<IWeaverAddin> addIns)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentNullException(nameof(assemblyPath));

            if (outputLog == null)
                outputLog = Logger.Default;

            if (addIns == null)
                throw new ArgumentNullException(nameof(addIns));

            if (!addIns.Any())
            {
                outputLog.Warning(nameof(AssemblyWeaver), "No add-ins were provided, weaving will be skipped since there is nothing to do");
                return true;
            }

            AbsolutePath assemblyLocation;

            if(PathUtility.HasRoot(assemblyPath))
            {
                assemblyLocation = new AbsolutePath(assemblyPath);
            }
            else
            {
                assemblyLocation = new AbsolutePath(WorkingDirectory);
                assemblyLocation /= assemblyPath;
            }

            if(!File.Exists(assemblyLocation))
            {
                throw new FileNotFoundException($"Unable to find the assembly {Path.GetFileName(assemblyPath)} at path '{assemblyPath}'.");
            }

            // All our input is validated now so we can move on to weaving! 


            return true;

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

            foreach(Type type in addIns)
            {
                if(!typeof(IWeaverAddin).IsAssignableFrom(type))
                {
                    Exception exception = new ArgumentException($"The type {type.FullName} does not inherit from the required type {typeof(IWeaverAddin)}");
                    outputLog.Exception(nameof(AssemblyWeaver), exception);
                }

                IWeaverAddin instance = (IWeaverAddin)Activator.CreateInstance(type, true);
                createdAddins.Add(instance);
            }

            return WeaveAssembly(assemblyPath, outputLog, addIns);
        }
    }
}

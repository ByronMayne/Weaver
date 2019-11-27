using System;
using Weaver.Contracts.Diagnostics;

namespace Weaver.Contracts
{
    /// <summary>
    /// The contract between an assembly and the weaving process. 
    /// </summary>
    public interface IAssemblyWeaver
    {
        /// <summary>
        /// Weaves an assembly from disk.
        /// </summary>
        /// <param name="assemblyPath">The system path to the assembly.</param>
        /// <param name="outputLog">The output log.</param>
        /// <param name="addIns">The add ins youu would like to run.</param>
        /// <returns>True if it's successful and false if it's not.</returns>
        bool WeaveAssembly(string assemblyPath, ILogger outputLog, params IWeaverAddin[] addIns);

        /// <summary>
        /// Weaves an assembly from disk.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="outputLog">The output log.</param>
        /// <param name="addIns">The add ins youu would like to run.</param>
        /// <returns>True if it's successful and false if it's not.</returns>
        bool WeaveAssembly(string assemblyPath, ILogger outputLog, params Type[] addIns);
    }
}

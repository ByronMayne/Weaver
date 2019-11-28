using Mono.Cecil;
using Seed.IO;
using System;

namespace Weaver.Contracts
{
    /// <summary>
    /// Defines a object that can cache multiple requests for looking up assemblies.
    /// </summary>
    public interface IAssemblyCache : IDisposable
    {
        /// <summary>
        /// Determines whether we have assembly currently cached the specified assembly path.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>
        ///   <c>true</c> if we have the specified assembly path; otherwise, <c>false</c>.
        /// </returns>
        bool Has(AbsolutePath assemblyPath);

        /// <summary>
        /// Removes the specified assembly from the cache.
        /// </summary>
        /// <param name="absolutePath">The absolute path to the assembly</param>
        bool Remove(AbsolutePath absolutePath);

        /// <summary>
        /// Gets the specified assembly definition from a given path.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>The AssemblyDefinition for the assembly at the given path</returns>
        AssemblyDefinition Get(AbsolutePath assemblyPath);

        /// <summary>
        /// Clears this instance of all assembly definitions
        /// </summary>
        void Clear();
    }
}

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Seed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using Weaver.Contracts;
using Weaver.Core;

namespace Weaver
{
    /// <summary>
    /// Allows us to cache the look-up of AssemblyDefinitions instead of loading them upon each request
    /// </summary>
    /// <seealso cref="Weaver.Contracts.IAssemblyCache" />
    class AssemblyCache : IAssemblyCache
    {
        private IDictionary<AbsolutePath, AssemblyDefinition> m_assemblyDefinitions;
        private IAssemblyResolver m_assemblyResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCache"/> class.
        /// </summary>
        public AssemblyCache()
        {
            m_assemblyResolver = new AssemblyResolver(this);
            m_assemblyDefinitions = new Dictionary<AbsolutePath, AssemblyDefinition>();
        }

        /// <summary>
        /// Clears this instance of all assembly definitions
        /// </summary>
        public void Clear()
        {
            foreach (AssemblyDefinition value in m_assemblyDefinitions.Values)
            {
                value.Dispose();
            }
            m_assemblyDefinitions.Clear();
        }

        /// <summary>
        /// Removes the specified assembly from the cache.
        /// </summary>
        /// <param name="absolutePath">
        /// The absolute path to the assembly
        /// </param>
        /// <returns>
        /// True if an assembly exists in the cache at that path and it was removed otherwise false.
        /// </returns>
        public bool Remove(AbsolutePath absolutePath)
        {
            if(Has(absolutePath))
            {
                AssemblyDefinition assemblyDefinition = Get(absolutePath, false);
                assemblyDefinition.Dispose();
                m_assemblyDefinitions.Remove(absolutePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the specified assembly definition from a given path.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>
        /// The AssemblyDefinition for the assembly at the given path
        /// </returns>
        public AssemblyDefinition Get(AbsolutePath assemblyPath, bool isReadOnly)
        {
            if (Has(assemblyPath))
                return m_assemblyDefinitions[assemblyPath];

            ISymbolReaderProvider readerProvider = GetSymbolReaderProvider(assemblyPath);

            ReaderParameters readerParameters = new ReaderParameters()
            {
                ReadSymbols = readerProvider != null,
                ReadWrite = !isReadOnly,
                AssemblyResolver = m_assemblyResolver,
                ReadingMode = ReadingMode.Deferred,
                ThrowIfSymbolsAreNotMatching = false,
                SymbolReaderProvider = readerProvider,
            };

            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);

            m_assemblyDefinitions[assemblyPath] = assemblyDefinition;

            return assemblyDefinition;
        }

        /// <summary>
        /// Gets the symbol reader provider based on the debug symbols that exists on disk.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>The instance that we are using to read symbols</returns>
        private ISymbolReaderProvider GetSymbolReaderProvider(AbsolutePath assemblyPath)
        {
            DebugSymbolType symbolType = DebugSymbolUtility.GetFromAssemblyPath(assemblyPath);

            switch(symbolType)
            {
                case DebugSymbolType.Mono: return new MdbReaderProvider(); 
                case DebugSymbolType.Program: return new PdbReaderProvider();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Determines whether we have assembly currently cached the specified assembly path.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>
        /// <c>true</c> if we have the specified assembly path; otherwise, <c>false</c>.
        /// </returns>
        public bool Has(AbsolutePath assemblyPath)
        {
            return m_assemblyDefinitions.ContainsKey(assemblyPath);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            Clear();
            m_assemblyDefinitions.Clear();
        }
    }
}

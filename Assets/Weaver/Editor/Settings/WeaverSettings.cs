using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Settings", fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        [SerializeField]
        private ComponentController m_Components;

        // Resolver
        private WeaverAssemblyResolver m_Resolver;

        public WeaverAssemblyResolver resolver
        {
            get { return m_Resolver; }
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Makes sure we initialize our instance.
            AssetDatabase.FindAssets("t:WeaverSettings");
        }

        private void OnEnable()
        {
            AssemblyUtility.PopulateAssemblyCache();
            // Create new resolver
            m_Resolver = new WeaverAssemblyResolver();
            // Create a new reader
            ReaderParameters readerParameters = new ReaderParameters();
            // Pass the reader our resolver 
            readerParameters.AssemblyResolver = m_Resolver;
            // Create a collection for all the assemblies that changed. 
            Collection<ModuleDefinition> changedModules = new Collection<ModuleDefinition>();

            if (m_WeavedAssemblies == null)
            {
                m_WeavedAssemblies = new List<WeavedAssembly>();
            }
            // Loop over them all
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                if (m_WeavedAssemblies[i].HasChanges())
                {
                    // We have a changed assembly so we need to get the defintion to modify. 
                    ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(m_WeavedAssemblies[i].relativePath, readerParameters);
                    // Add it to our list
                    changedModules.Add(moduleDefinition);
                }
            }
            // Initialize our component manager
            m_Components.Initialize(this);
            // Visit Modules
            m_Components.VisitModules(changedModules);
        }
    }
}

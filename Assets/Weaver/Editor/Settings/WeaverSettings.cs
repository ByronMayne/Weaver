using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;
using System.Reflection;

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
            Instance();
        }
        
        /// <summary>
        /// Gets the instance of our Settings if it exists. Returns null
        /// if no instance was created. 
        /// </summary>
        public static WeaverSettings Instance()
        {
            // Find all settings
            string[] guids = AssetDatabase.FindAssets("t:WeaverSettings");
            // Load them all
            for (int i = 0; i < guids.Length; i++)
            {
                // Convert our path
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                // Load it
               return AssetDatabase.LoadAssetAtPath<WeaverSettings>(assetPath);
            }
            return null;
        }

        [PostProcessScene]
        public static void PostprocessScene()
        {
            // Only run this code if we are building the player 
            if (BuildPipeline.isBuildingPlayer)
            {
                // Get our current scene 
                Scene scene = SceneManager.GetActiveScene();
                // If we are the first scene (we only want to run once)
                if (scene.IsValid() && scene.buildIndex == 0)
                {
                    // Find all settings
                    string[] guids = AssetDatabase.FindAssets("t:WeaverSettings");
                    // Load them all
                    if (guids.Length > 0)
                    {
                        // Convert our path
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        // Load it
                        WeaverSettings settings = AssetDatabase.LoadAssetAtPath<WeaverSettings>(assetPath);
                        // Invoke
                        settings.WeaveUpdatedAssemblies();
                    }
                }
            }
        }

        /// <summary>
        /// Inovked when our module is first created and turned on
        /// </summary>
        private void OnEnable()
        {
            AssemblyUtility.PopulateAssemblyCache();
            // Subscribe to the before reload event so we can modify the assemblies!
            AssemblyReloadEvents.beforeAssemblyReload += WeaveUpdatedAssemblies;
        }

        /// <summary>
        /// Used to modifiy all existing assemblies on disk. 
        /// </summary>
        private void WeaveUpdatedAssemblies()
        {
            // Create a collection for all the assemblies that changed. 
            Collection<ModuleDefinition> changedModules = new Collection<ModuleDefinition>();

            if (m_WeavedAssemblies == null)
            {
                m_WeavedAssemblies = new List<WeavedAssembly>();
            }

            List<WeavedAssembly> assembliesToWrite = new List<WeavedAssembly>();
            // Loop over them all
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                if (m_WeavedAssemblies[i].HasChanges())
                {

                    assembliesToWrite.Add(m_WeavedAssemblies[i]);
                }
            }

            WeaveAssemblies(assembliesToWrite);
        }

        /// <summary>
        /// Takes in a collection of assemblies and starts the weaving process for
        /// all of them. 
        /// </summary>
        public void WeaveAssemblies(IList<WeavedAssembly> assemblies)
        {
            AssemblyUtility.PopulateAssemblyCache();
            // Create new resolver
            m_Resolver = new WeaverAssemblyResolver();
            // Create a new reader
            ReaderParameters readerParameters = new ReaderParameters();
            // Pass the reader our resolver 
            readerParameters.AssemblyResolver = m_Resolver;
            // Tell the reader to look at symbols so we can get line numbers for errors, warnings, and logs.
            readerParameters.ReadSymbols = true;
            // Create a list of definitions
            Collection<ModuleDefinition> editingModules = new Collection<ModuleDefinition>();
            for (int i = 0; i < assemblies.Count; i++)
            {
                // We have a changed assembly so we need to get the defintion to modify. 
                ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(assemblies[i].GetSystemPath(), readerParameters);
                // Add it to our list
                editingModules.Add(moduleDefinition);
            }
            // Initialize our component manager
            m_Components.Initialize(this);
            // Visit Modules
            m_Components.VisitModules(editingModules);
            // Save
            for (int i = 0; i < assemblies.Count; i++)
            {
                editingModules[i].Write(assemblies[i].GetSystemPath());
            }
            assemblies.Clear();
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System;
using System.Diagnostics;
using System.Text;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Settings", fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject, ILogable
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        [SerializeField]
        private ComponentController m_Components;

        [SerializeField]
        [Tooltip("If true whenever one of our weaved assemblies changes will run the weaving process.")]
        private bool m_RunAutomatically;

        [SerializeField]
        private Log m_Log;

        [SerializeField]
        private Stopwatch m_Timer;

        // Resolver
        private WeaverAssemblyResolver m_Resolver;

        public WeaverAssemblyResolver resolver
        {
            get { return m_Resolver; }
        }

        public ComponentController componentController
        {
            get { return m_Components; }
        }

        Object ILogable.context
        {
            get { return this; }
        }

        string ILogable.label
        {
            get { return "WeaverSettings"; }
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
            WeaverSettings settings = null;
            // Find all settings
            string[] guids = AssetDatabase.FindAssets("t:WeaverSettings");
            // Load them all
            for (int i = 0; i < guids.Length; i++)
            {
                // Convert our path
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                // Load it 
                settings = AssetDatabase.LoadAssetAtPath<WeaverSettings>(assetPath);
            }
            return settings;
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
                        settings.CheckForAssemblyModifications();
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when our module is first created and turned on
        /// </summary>
        private void OnEnable()
        {
            m_Log.context = this;
            // Subscribe to the before reload event so we can modify the assemblies!
            m_Log.Info("Subscribing to Assembly Reload", true);
            AssemblyReloadEvents.beforeAssemblyReload += CheckForAssemblyModifications;
        }


        /// <summary>
        /// Used to modify all existing assemblies on disk. 
        /// </summary>
        private void CheckForAssemblyModifications()
        {
            m_Timer = new Stopwatch();
            m_Timer.Start();
            try
            {

                m_Log.Clear();
                m_Log.Info("Checking for assembly modifications.", false);
                if (m_WeavedAssemblies == null)
                {
                    m_WeavedAssemblies = new List<WeavedAssembly>();
                }

                if (!m_RunAutomatically)
                {
                    m_Log.Info("Automatic weaving aborted due to RunAutomaticlly being turned off.", false);
                    // We don't want to run if the users said not too.
                    return;
                }

                List<WeavedAssembly> assembliesToWrite = new List<WeavedAssembly>();
                // Loop over them all

                for (int i = 0; i < m_WeavedAssemblies.Count; i++)
                {
                    if (m_WeavedAssemblies[i].HasChanges())
                    {
                        m_Log.Info("Assembly at path <i>" + m_WeavedAssemblies[i].relativePath + "</i> had modifications.", false);
                        assembliesToWrite.Add(m_WeavedAssemblies[i]);
                    }
                }

                WeaveAssemblies(assembliesToWrite);
            }
            catch (Exception e)
            {
                m_Timer.Stop();
                StringBuilder log = new StringBuilder();
                log.AppendLine("An exception was thrown while weaving assemblies.");
                log.AppendLine(e.ToString());
                log.Append("Total elapsed milliseconds :");
                log.AppendLine(m_Timer.ElapsedMilliseconds.ToString());
                m_Log.Error("Exception was thrown while weaving assemblies. " + e.ToString(), true);
            }
        }

        /// <summary>
        /// Takes in a collection of assemblies and starts the weaving process for
        /// all of them. 
        /// </summary>
        private void WeaveAssemblies(IList<WeavedAssembly> assemblies)
        {
            m_Log.Info("Populating Assembly Cache", false);
            AssemblyUtility.PopulateAssemblyCache();
            // Create new resolver
            m_Resolver = new WeaverAssemblyResolver();
            // Create a new reader
            m_Log.Info("Creating Reader Parameters", false);
            ReaderParameters readerParameters = new ReaderParameters();
            // Pass the reader our resolver 
            readerParameters.AssemblyResolver = m_Resolver;
            // Tell the reader to look at symbols so we can get line numbers for errors, warnings, and logs.
            readerParameters.ReadSymbols = true;
            // Create a list of definitions
            Collection<ModuleDefinition> editingModules = new Collection<ModuleDefinition>();
            for (int i = 0; i < assemblies.Count; i++)
            {
                // We have a changed assembly so we need to get the definition to modify. 
                m_Log.Info("Creating ModuleDefinition for <i>" + assemblies[i].relativePath + "</i>.", false);
                ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(assemblies[i].GetSystemPath(), readerParameters);
                // Add it to our list
                editingModules.Add(moduleDefinition);
            }
            m_Log.Info("Initializing Components.", false);
            // Initialize our component manager
            m_Components.Initialize(this);
            // Visit Modules
            m_Log.Info("Visiting Modules.", false);
            m_Components.VisitModules(editingModules);
            // Save
            for (int i = 0; i < assemblies.Count; i++)
            {
                m_Log.Info("Writing Module <i>" + assemblies[i].relativePath + "</i> to disk.", false);
                editingModules[i].Write(assemblies[i].GetSystemPath());
            }
            assemblies.Clear();
            m_Log.Info("Weaving Completed Successful. Total elapsed milliseconds : " + m_Timer.ElapsedMilliseconds.ToString(), false);
        }
    }
}

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
using JetBrains.Annotations;
using Weaver.Analytics;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Settings", fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject, ILogable
    {
        public const string VERSION = "3.3.0";

        [SerializeField]
        [Tooltip("This is evaluated before Weaver runs to check if it should execute. The symbol expression must come out to be true")]
        private ScriptingSymbols m_RequiredScriptingSymbols;

        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        [SerializeField]
        [UsedImplicitly]
        private ComponentController m_Components;

        [SerializeField]
        [UsedImplicitly]
        private bool m_IsEnabled = true; // m_Enabled is used by Unity and throws errors (even if scriptable objects don't have that field) 

        [SerializeField]
        [UsedImplicitly]
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

        [UsedImplicitly]
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
        [UsedImplicitly]
        private void OnEnable()
        {
            if (m_Log == null)
            {
                m_Log = new Log(this);
            }

            if (m_Components == null)
            {
                m_Components = new ComponentController();
            }

            // Enable all our components 
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                m_WeavedAssemblies[i].OnEnable();
            }
            m_Log.context = this;
            // Subscribe to the before reload event so we can modify the assemblies!
            m_Log.Info("Weaver Settings", "Subscribing to next assembly reload.", false);
            AssemblyUtility.PopulateAssemblyCache();
            m_Components.SetOwner(this);
#if UNITY_2017_1_OR_NEWER
            AssemblyReloadEvents.beforeAssemblyReload += CheckForAssemblyModifications;
#else
            m_Log.Warning("Dynamic Assembly Reload not support until Unity 2017. Enter play mode to reload assemblies to see the effects of Weaving.", false);
#endif
            WeaverAnalytics.OnSettingsEnabled(this);
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
                m_Log.Info("Weaver Settings", "Checking for assembly modifications.", false);
                if (m_WeavedAssemblies == null)
                {
                    m_WeavedAssemblies = new List<WeavedAssembly>();
                }

                if (!m_IsEnabled)
                {
                    m_Log.Info("Weaver Settings", "Weaving aborted due to being Disabled.", false);
                    return;
                }

                if(!m_RequiredScriptingSymbols.isActive)
                {
                    m_Log.Info("Weaver Settings", "Weaving aborted due to required scripting symbols not being defined.", false);
                    return;
                }

                List<WeavedAssembly> assembliesToWrite = new List<WeavedAssembly>();
                // Loop over them all

                for (int i = 0; i < m_WeavedAssemblies.Count; i++)
                {
                    if (m_WeavedAssemblies[i].HasChanges())
                    {
                        m_Log.Info("Weaver Settings", "Assembly at path <i>" + m_WeavedAssemblies[i].relativePath + "</i> had modifications.", false);
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
                m_Log.Error("Weaver Settings", "Exception was thrown while weaving assemblies. " + e.ToString(), true);
            }
        }

        /// <summary>
        /// Takes in a collection of assemblies and starts the weaving process for
        /// all of them. 
        /// </summary>
        private void WeaveAssemblies(IList<WeavedAssembly> assemblies)
        {
            try
            {

                m_Log.Info("Weaver Settings", "Populating Assembly Cache", false);
                AssemblyUtility.PopulateAssemblyCache();
                // Create new resolver
                m_Resolver = new WeaverAssemblyResolver();
                // Create a new reader
                m_Log.Info("Weaver Settings", "Creating Reader Parameters", false);
                ReaderParameters readerParameters = new ReaderParameters();
                // Pass the reader our resolver 
                readerParameters.AssemblyResolver = m_Resolver;
                // Tell the reader to look at symbols so we can get line numbers for errors, warnings, and logs.
                readerParameters.ReadSymbols = true;
                // Create our writer
                WriterParameters writerParameters = new WriterParameters();
                // We do want to write our symbols
                writerParameters.WriteSymbols = true;
                // Create a list of definitions
                Collection<ModuleDefinition> editingModules = new Collection<ModuleDefinition>();
                for (int i = 0; i < assemblies.Count; i++)
                {
                    // We have a changed assembly so we need to get the definition to modify. 
                    m_Log.Info("Weaver Settings", "Creating ModuleDefinition for <i>" + assemblies[i].relativePath + "</i>.", false);
                    ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(assemblies[i].GetSystemPath(), readerParameters);
                    // Add it to our list
                    editingModules.Add(moduleDefinition);
                }
                m_Log.Info("Weaver Settings", "Initializing Components.", false);
                // Initialize our component manager
                m_Components.Initialize(this);
                // Visit Modules
                m_Log.Info("Weaver Settings", "Visiting Modules.", false);
                m_Components.VisitModules(editingModules, m_Log);
                // Save
                for (int i = 0; i < assemblies.Count; i++)
                {
                    m_Log.Info("Weaver Settings", "Writing Module <i>" + assemblies[i].relativePath + "</i> to disk.", false);
                    editingModules[i].Write(assemblies[i].GetSystemPath(), writerParameters);
                }
                assemblies.Clear();
                m_Log.Info("Weaver Settings", "Weaving Successfully Completed", false);

                // Stats
                m_Log.Info("Statistics", "Weaving Time ms: " + m_Timer.ElapsedMilliseconds, false);
                m_Log.Info("Statistics", "Modules Visited: " + m_Components.totalModulesVisited, false);
                m_Log.Info("Statistics", "Types Visited: " + m_Components.totalTypesVisited, false);
                m_Log.Info("Statistics", "Methods Visited: " + m_Components.totalMethodsVisited, false);
                m_Log.Info("Statistics", "Fields Visited: " + m_Components.totalFieldsVisited, false);
                m_Log.Info("Statistics", "Properties Visited: " + m_Components.totalPropertiesVisited, false);
                WeaverAnalytics.SendTiming("WeaveStats", "Elapsed Time", m_Timer.ElapsedMilliseconds);
                WeaverAnalytics.SendEvent("WeaveStats", "Modules Visited", componentController.totalModulesVisited.ToString(), null);
                WeaverAnalytics.SendEvent("WeaveStats", "Types Visited", componentController.totalTypesVisited.ToString(), null);
                WeaverAnalytics.SendEvent("WeaveStats", "Methods Visited", componentController.totalMethodsVisited.ToString(), null);
                WeaverAnalytics.SendEvent("WeaveStats", "Fields Visited", componentController.totalFieldsVisited.ToString(), null);
                WeaverAnalytics.SendEvent("WeaveStats", "Properties Visited", componentController.totalPropertiesVisited.ToString(), null);
            }
            catch (Exception e)
            {
                WeaverAnalytics.SendException(e.ToString(), true);
                throw e;
            }
        }

        [UsedImplicitly]
        private void OnValidate()
        {
            m_RequiredScriptingSymbols.ValidateSymbols();
        }
    }
}

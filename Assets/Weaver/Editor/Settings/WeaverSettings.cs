using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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

        [UsedImplicitly]
        private Log m_Log;

        [SerializeField]
        private Stopwatch m_Timer;

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
                        settings.WeaveModifiedAssemblies();
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
            AssemblyUtility.PopulateAssemblyCache();

            if (m_Log == null)
            {
                m_Log = new Log(this);
            }

            if (m_Components == null)
            {
                m_Components = new ComponentController();
            }

            if (m_WeavedAssemblies == null)
            {
                m_WeavedAssemblies = new List<WeavedAssembly>();
            }

            m_Components.SetOwner(this);
            m_RequiredScriptingSymbols.ValidateSymbols();

            // Enable all our components 
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                m_WeavedAssemblies[i].OnEnable();
            }
            m_Timer = new Stopwatch();
            m_Log.context = this;
            // Subscribe to the before reload event so we can modify the assemblies!
            m_Log.Info("Weaver Settings", "Subscribing to next assembly reload.", false);
            AssemblyUtility.PopulateAssemblyCache();

#if UNITY_2019_1_OR_NEWER
            CompilationPipeline.assemblyCompilationFinished += ComplicationComplete;
#elif UNITY_2017_1_OR_NEWER
            AssemblyReloadEvents.beforeAssemblyReload += WeaveModifiedAssemblies;
#else
            m_Log.Warning("Dynamic Assembly Reload not support until Unity 2017. Enter play mode to reload assemblies to see the effects of Weaving.", false);
#endif
        }

#if UNITY_2019_1_OR_NEWER
        /// <summary>
        /// Invoked whenever one of our assemblies has compelted compliling.  
        /// </summary>
        private void ComplicationComplete(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            WeaveAssembly(assemblyPath);
        }
#endif

        /// <summary>
        /// Loops over all changed assemblies and starts the weaving process for each. 
        /// </summary>
        private void WeaveModifiedAssemblies()
        {
            IList<string> assemblies = m_WeavedAssemblies
                     .Where(a => a.HasChanges())
                     .Where(a => a.IsActive)
                     .Select(a => a.relativePath)
                     .ToArray();


            foreach (string assembly in assemblies)
            {
                WeaveAssembly(assembly);
            }
        }


        /// <summary>
        /// Returns back an instance of our symbol reader for 
        /// </summary>
        /// <returns></returns>
        private static ReaderParameters GetReaderParameters()
        {
            return new ReaderParameters()
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = new WeaverAssemblyResolver(),
                ReadSymbols = true,
                SymbolReaderProvider = new PdbReaderProvider()
            };
        }

        /// <summary>
        /// Returns back the instance of the symbol writer provide.
        /// </summary>
        private static WriterParameters GetWriterParameters()
        {
            return new WriterParameters()
            {
                WriteSymbols = true,
                SymbolWriterProvider = new PdbWriterProvider()
            };
        }

        /// <summary>
        /// Invoked for each assemby that has been compiled. 
        /// </summary>
        private void WeaveAssembly(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                return;
            }

            string name = Path.GetFileNameWithoutExtension(assemblyPath);

            m_Log.Info(name, "Starting", false);
            if (!m_IsEnabled)
            {
                m_Log.Info(name, "Aborted due to weaving being disabled.", false);
                return;
            }

            if (!m_RequiredScriptingSymbols.isActive)
            {
                m_Log.Info(name, "Aborted due to non-matching script symbols.", false);
                return;
            }

            string filePath = Path.Combine(Constants.ProjectRoot, assemblyPath);

            if (!File.Exists(filePath))
            {
                m_Log.Error(name, "Unable to find assembly at path '" + filePath + "'.", true);
                return;
            }

            using (FileStream assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(assemblyStream, GetReaderParameters()))
                {
                    m_Components.Initialize(this);

                    m_Components.VisitModule(moduleDefinition, m_Log);

                    // Save
                    WriterParameters writerParameters = new WriterParameters()
                    {
                        WriteSymbols = true,
                        SymbolWriterProvider = new Mono.Cecil.Pdb.NativePdbWriterProvider()
                    };

                    moduleDefinition.Write(GetWriterParameters());
                }
            }

            m_Log.Info("Weaver Settings", "Weaving Successfully Completed", false);

            // Stats
            m_Log.Info(name, "Time ms: " + m_Timer.ElapsedMilliseconds, false);
            m_Log.Info(name, "Types: " + m_Components.totalTypesVisited, false);
            m_Log.Info(name, "Methods: " + m_Components.totalMethodsVisited, false);
            m_Log.Info(name, "Fields: " + m_Components.totalFieldsVisited, false);
            m_Log.Info(name, "Properties: " + m_Components.totalPropertiesVisited, false);
            m_Log.Info(name, "Complete", false);

            // save any changes to our weavedAssembly objects
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }


        [UsedImplicitly]
        private void OnValidate()
        {
            m_RequiredScriptingSymbols.ValidateSymbols();
        }
    }
}


using Seed.IO;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Weaver.Unity
{
    /// <summary>
    /// This is the entrypoint from all the the Unity callbacks that
    /// Weaver hooks into. 
    /// </summary>
    public static class EntryPoint
    {
        private static UnityWeaver m_weaver; 

        [InitializeOnLoadMethod]
        private static void Setup()
        {
            m_weaver = new UnityWeaver();
            CompilationPipeline.assemblyCompilationFinished += ComplicationComplete;
        }

        [PostProcessScene]
        private static void PostProcessScene()
        {
            // Only run this code if we are building the player 
            if (BuildPipeline.isBuildingPlayer)
            {
                // Get our current scene 
                Scene scene = SceneManager.GetActiveScene();
                // If we are the first scene (we only want to run once)
                if (scene.IsValid() && scene.buildIndex == 0)
                {
                    AbsolutePath scriptAssembliesPath = new AbsolutePath(Application.dataPath).GetParent();
                    scriptAssembliesPath /= "Library";
                    scriptAssembliesPath /= "ScriptAssemblies";

                    foreach(AbsolutePath assemblyPath in Directory.GetFiles(scriptAssembliesPath, "*.dll"))
                    {
                        m_weaver.WeaveAssembly(assemblyPath);
                    }
                }
            }
        }

        /// <summary>
        /// Invoked whenever one of our assemblies has compelted compliling.  
        /// </summary>
        private static void ComplicationComplete(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            AbsolutePath absolutePath =  new AbsolutePath(Application.dataPath).GetParent();
            absolutePath /= assemblyPath;
            m_weaver.WeaveAssembly(absolutePath);
        }
    }
}

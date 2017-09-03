using UnityEngine;
using UnityEditor;

namespace Weaver
{
    [InitializeOnLoad]
    internal class WeaverSettings : SerializedWeaver<WeaverSettings>
    {
        /// <summary>
        /// Invoked when Unity loads our assemblies 
        /// </summary>
        static WeaverSettings()
        {
            EditorApplication.delayCall += LoadInstance;
        }

        protected void OnEnable()
        {
            Debug.Log("On Enable");
        }
    }
}

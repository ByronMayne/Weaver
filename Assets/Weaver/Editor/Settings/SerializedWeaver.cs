using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

namespace Weaver
{
    internal abstract class SerializedWeaver<T> : Editor where T : SerializedWeaver<T> 
    {
        private const string FILE_NAME = "Weaver";
        private const string EXTENSION = ".asset";

        private static T m_Instance;

        /// <summary>
        /// Returns the save path of this object on disk. 
        /// </summary>
        protected static string savePath
        {
            get
            {
                return Application.dataPath.Replace("/Assets", "/ProjectSettings/" + FILE_NAME + EXTENSION);
            }
        }

        /// <summary>
        /// Loads the current instance of Weaver from disk
        /// or creates a new one. 
        /// </summary>
        protected static void LoadInstance()
        {
            if (m_Instance == null)
            {
                // Try to find all instances
                T[] instances = Resources.FindObjectsOfTypeAll<T>();
                // Loop over them all
                for(int i = 0; i < instances.Length; i++)
                {
                    if(i == 0)
                    {
                        m_Instance = instances[i];
                    }
                    else
                    {
                        // Extra editors are being created so we must delete them
                        DestroyImmediate(instances[i]);
                        Debug.LogWarning("Extra instance of Weaver detected. Deleting");
                    }
                }
                // Check if it's null
                if (m_Instance == null)
                {
                    // Check if a saved files exists
                    if (File.Exists(savePath))
                    {
                        // Load the file from disk
                        Object[] loadedObjects = InternalEditorUtility.LoadSerializedFileAndForget(savePath);
                        // Validate that we loaded something
                        if (loadedObjects.Length > 0)
                        {
                            // Check the first objects type
                            if (loadedObjects[0] is T)
                            {
                                // We are good
                                m_Instance = (T)loadedObjects[0];
                                return;
                            }
                        }
                        // If we made it to this point the file that was saved was invalid. So remove it
                        File.Delete(savePath);
                    }
                    // If we made it to this point we don't have an instance
                    m_Instance = CreateInstance<T>();
                    Debug.Log(typeof(SerializedWeaver<T>).FullName);
                    // Save it to disk to make sure it's there for next time
                    m_Instance.Save();
                }
            }
        }

        /// <summary>
        /// Takes this instance and writes it to disk. 
        /// </summary>
        private void Save()
        {
            // Populate our list of things we are saving
            Object[] objectsToSave = new Object[] { this };
            // Write to disk
            InternalEditorUtility.SaveToSerializedFileAndForget(objectsToSave, savePath, true);
        }

        /// <summary>
        /// Invoked when our instance gets unloaded. 
        /// </summary>
        protected virtual void OnDisable()
        {
            // Apply all changes and force a write to disk. 
            Save();
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Mono.Cecil;
using UnityEngine.Serialization;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Settings", fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject // SerializedWeaver<WeaverSettings>
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        [SerializeField, FormerlySerializedAs("m_Addins")]
        private List<WeaverExtension> m_Extensions;

        // Watchers
        private List<FileSystemWatcher> m_AssemblyWatches;

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
            Debug.Log("Enabled");
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                // Initialize our assembly
                m_WeavedAssemblies[i].CheckForChanges(OnAssemblyChanged);
            }
        }


        /// <summary>
        /// Invoked when ever one of the assemblies we are
        /// watching has changed. 
        /// </summary>
        private void OnAssemblyChanged(WeavedAssembly weavedAssembly)
        {
            // Create new resolver
            m_Resolver = new WeaverAssemblyResolver();

            // Clean up any missing addins so we don't have to null check all the time
            for (int i = m_Extensions.Count - 1; i >= 0; i--)
            {
                if (m_Extensions[i] == null)
                {
                    m_Extensions.RemoveAt(i);
                }
                else
                {
                    m_Extensions[i].Initialize(this);
                }
            }
            // Save our write times to disk.
            EditorUtility.SetDirty(this);
            // Create reader settings
            ReaderParameters readerParameters = new ReaderParameters();
            readerParameters.AssemblyResolver = m_Resolver;
            // Load our definition
            ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(weavedAssembly.filePath, readerParameters);
            // Call init on each of our addins
            for (int i = 0; i < m_Extensions.Count; i++)
            {
                if (m_Extensions[i].EffectsDefintion(DefinitionType.Module))
                {
                    m_Extensions[i].VisitModule(moduleDefinition);
                }
            }
            // TYPES
            foreach(TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                for (int i = 0; i < m_Extensions.Count; i++)
                {
                    if (m_Extensions[i].EffectsDefintion(DefinitionType.Type))
                    {
                        m_Extensions[i].VisitType(typeDefinition);
                    }
                }

                // METHODS
                foreach(MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    for (int i = 0; i < m_Extensions.Count; i++)
                    {
                        if (m_Extensions[i].EffectsDefintion(DefinitionType.Method))
                        {
                            m_Extensions[i].VisitMethod(methodDefinition);
                        }
                    }
                }

                // PROPERTIES
                foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
                {
                    for (int i = 0; i < m_Extensions.Count; i++)
                    {
                        if (m_Extensions[i].EffectsDefintion(DefinitionType.Property))
                        {
                            m_Extensions[i].VisitProperty(propertyDefinition);
                        }
                    }
                }

                // FIELDS
                foreach (FieldDefinition fieldDefinition in typeDefinition.Fields)
                {
                    for (int i = 0; i < m_Extensions.Count; i++)
                    {
                        if (m_Extensions[i].EffectsDefintion(DefinitionType.Field))
                        {
                            m_Extensions[i].VisitField(fieldDefinition);
                        }
                    }
                }
            }
            // Write the values to disk
            moduleDefinition.Write(weavedAssembly.filePath);
        }
    }
}

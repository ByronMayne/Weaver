using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Settings", fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject // SerializedWeaver<WeaverSettings>
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        [SerializeField]
        private List<WeaverPlugin> m_Addins;

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
        }

        private void OnEnable()
        {
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                m_WeavedAssemblies[i].Initialize();
                m_WeavedAssemblies[i].AddListener(OnAssemblyChanged);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                m_WeavedAssemblies[i].RemoveListener(OnAssemblyChanged);
            }
        }

        /// <summary>
        /// Invoked when ever one of the assemblies we are
        /// watching has changed. 
        /// </summary>
        private void OnAssemblyChanged(string filePath)
        {
            // Create new resolver
            m_Resolver = new WeaverAssemblyResolver();

            // Clean up any missing addins so we don't have to null check all the time
            for (int i = m_Addins.Count - 1; i >= 0; i--)
            {
                if (m_Addins[i] == null)
                {
                    m_Addins.RemoveAt(i);
                }
                else
                {
                    m_Addins[i].Initialize(this);
                }
            }
            // Save our write times to disk.
            EditorUtility.SetDirty(this);
            // Create reader settings
            ReaderParameters readerParameters = new ReaderParameters();
            readerParameters.AssemblyResolver = m_Resolver;
            // Load our definition
            ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(filePath, readerParameters);
            // Call init on each of our addins
            for (int i = 0; i < m_Addins.Count; i++)
            {
                if (m_Addins[i].EffectsDefintion(DefinitionType.Module))
                {
                    m_Addins[i].VisitModule(moduleDefinition);
                }
            }
            // TYPES
            foreach(TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                for (int i = 0; i < m_Addins.Count; i++)
                {
                    if (m_Addins[i].EffectsDefintion(DefinitionType.Type))
                    {
                        m_Addins[i].VisitType(typeDefinition);
                    }
                }

                // METHODS
                foreach(MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    for (int i = 0; i < m_Addins.Count; i++)
                    {
                        if (m_Addins[i].EffectsDefintion(DefinitionType.Method))
                        {
                            m_Addins[i].VisitMethod(methodDefinition);
                        }
                    }
                }

                // PROPERTIES
                foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
                {
                    for (int i = 0; i < m_Addins.Count; i++)
                    {
                        if (m_Addins[i].EffectsDefintion(DefinitionType.Property))
                        {
                            m_Addins[i].VisitProperty(propertyDefinition);
                        }
                    }
                }

                // FIELDS
                foreach (FieldDefinition fieldDefinition in typeDefinition.Fields)
                {
                    for (int i = 0; i < m_Addins.Count; i++)
                    {
                        if (m_Addins[i].EffectsDefintion(DefinitionType.Field))
                        {
                            m_Addins[i].VisitField(fieldDefinition);
                        }
                    }
                }
            }
            // Write the values to disk
            moduleDefinition.Write(filePath);
        }
    }
}

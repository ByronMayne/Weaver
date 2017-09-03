using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Weaver
{
    [CustomEditor(typeof(WeaverSettings))]
    public class WeaverSettingsEditor : Editor
    {
        private SerializedProperty m_WeavedAssemblies;
        private ReorderableList m_WeavedAssembliesList;

        // Labels
        private GUIContent m_RefreshAssembliesLabel;

        // Assemblies
        private List<string> m_AssemblyCache;



        public void OnEnable()
        {
            m_WeavedAssemblies = serializedObject.FindProperty("m_WeavedAssemblies");
            m_WeavedAssembliesList = new ReorderableList(serializedObject, m_WeavedAssemblies);
            m_WeavedAssembliesList.drawElementCallback += OnDrawWeavedAssemblyElement;
            m_WeavedAssembliesList.onAddCallback += OnWeavedAssemblyElementAdded;

            // Labels 
            m_RefreshAssembliesLabel = new GUIContent("Refresh Assemblies");

            PopulateAssembliesCache();
        }



        public override void OnInspectorGUI()
        {
            m_WeavedAssembliesList.DoLayoutList();
        }

        protected override void OnHeaderGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button(m_RefreshAssembliesLabel, EditorStyles.toolbarButton))
                {
                    PopulateAssembliesCache();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        private void OnDrawWeavedAssemblyElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty indexProperty = m_WeavedAssemblies.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, indexProperty);
        }

        private void OnWeavedAssemblyElementAdded(ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            for (int x = 0; x < m_AssemblyCache.Count; x++)
            {
                bool foundMatch = false;
                for (int y = 0; y < m_WeavedAssemblies.arraySize; y++)
                {
                    SerializedProperty current = m_WeavedAssemblies.GetArrayElementAtIndex(y);
                    SerializedProperty assetPath = current.FindPropertyRelative("m_FilePath");
                    if(string.Equals(m_AssemblyCache[x], assetPath.stringValue, StringComparison.Ordinal))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if(!foundMatch)
                {
                    string name = Path.GetFileName(m_AssemblyCache[x]);
                    GUIContent content = new GUIContent(name);
                    menu.AddItem(content, false, AddWeavedAssembly, m_AssemblyCache[x]);
                }
                menu.ShowAsContext();
            }
        }

        private void AddWeavedAssembly(object path)
        {
            m_WeavedAssemblies.arraySize++;
            SerializedProperty weaved = m_WeavedAssemblies.GetArrayElementAtIndex(m_WeavedAssemblies.arraySize - 1);
            weaved.FindPropertyRelative("m_FilePath").stringValue = (string)path;
            weaved.FindPropertyRelative("m_Enabled").boolValue = true;
            serializedObject.ApplyModifiedProperties();
        }

        private void PopulateAssembliesCache()
        {
            m_AssemblyCache = (List<string>)AssemblyUtility.GetUserAssemblies();
        }
    }
}

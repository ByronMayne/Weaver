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
        // Properties
        private SerializedProperty m_WeavedAssemblies;
        private SerializedProperty m_Components;

        // Lists
        private ReorderableList m_WeavedAssembliesList;
        private ReorderableList m_ComponentsList;

        // Labels
        private GUIContent m_RefreshAssembliesLabel;
        private GUIContent m_WeavedAssemblyHeaderLabel;
        private GUIContent m_COmponentsHeader;

        // Assemblies
        private List<string> m_AssemblyCache;



        public void OnEnable()
        {
            m_WeavedAssemblies = serializedObject.FindProperty("m_WeavedAssemblies");
            m_WeavedAssembliesList = new ReorderableList(serializedObject, m_WeavedAssemblies);
            m_WeavedAssembliesList.drawElementCallback += OnWeavedAssemblyDrawElement;
            m_WeavedAssembliesList.onAddCallback += OnWeavedAssemblyElementAdded;
            m_WeavedAssembliesList.drawHeaderCallback += OnWeavedAssemblyHeader;

            m_Components = serializedObject.FindProperty("m_Components");
            m_ComponentsList = new ReorderableList(serializedObject, m_Components);
            m_ComponentsList.drawElementCallback += OnDrawComponentElement;
            m_ComponentsList.drawHeaderCallback += OnComponentHeader;
            m_ComponentsList.onAddCallback += OnAddComponet;

            // Labels 
            m_RefreshAssembliesLabel = new GUIContent("Refresh Assemblies");
            m_WeavedAssemblyHeaderLabel = new GUIContent("Weaved Assemblies");
            m_COmponentsHeader = new GUIContent("Components");

            PopulateAssembliesCache();
        }
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Weaver", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            m_ComponentsList.DoLayoutList();
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

        #region -= Weaved Assemblies =-
        private void OnComponentHeader(Rect rect)
        {
            GUI.Label(rect, m_COmponentsHeader);
        }

        private void OnDrawComponentElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty current = m_Components.GetArrayElementAtIndex(index);

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.PropertyField(rect, current, GUIContent.none);
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }


        private void OnAddComponet(ReorderableList list)
        {
            list.serializedProperty.arraySize++;
            m_Components.GetArrayElementAtIndex(m_Components.arraySize - 1).objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region -= Weaved Assemblies =-
        private void OnWeavedAssemblyDrawElement(Rect rect, int index, bool isActive, bool isFocused)
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
                    if (string.Equals(m_AssemblyCache[x], assetPath.stringValue, StringComparison.Ordinal))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    string name = Path.GetFileName(m_AssemblyCache[x]);
                    GUIContent content = new GUIContent(name);
                    menu.AddItem(content, false, OnWeavedAssemblyAdded, m_AssemblyCache[x]);
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("[All Assemblies Added]"));
            }

            menu.ShowAsContext();
        }

        private void OnWeavedAssemblyHeader(Rect rect)
        {
            GUI.Label(rect, m_WeavedAssemblyHeaderLabel);
        }

        private void OnWeavedAssemblyAdded(object path)
        {
            m_WeavedAssemblies.arraySize++;
            SerializedProperty weaved = m_WeavedAssemblies.GetArrayElementAtIndex(m_WeavedAssemblies.arraySize - 1);
            weaved.FindPropertyRelative("m_FilePath").stringValue = (string)path;
            weaved.FindPropertyRelative("m_Enabled").boolValue = true;
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        private void PopulateAssembliesCache()
        {
            m_AssemblyCache = (List<string>)AssemblyUtility.GetUserAssemblies();
        }
    }
}

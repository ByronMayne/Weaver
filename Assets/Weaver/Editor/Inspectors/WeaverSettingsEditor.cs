using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Weaver.Editors
{
    [CustomEditor(typeof(WeaverSettings))]
    public class WeaverSettingsEditor : Editor
    {
        // Properties
        private SerializedProperty m_WeavedAssemblies;
        private SerializedProperty m_Components;

        // Lists
        private ReorderableList m_WeavedAssembliesList;

        // Labels
        private GUIContent m_WeavedAssemblyHeaderLabel;

        public void OnEnable()
        {
            m_WeavedAssemblies = serializedObject.FindProperty("m_WeavedAssemblies");
            m_Components = serializedObject.FindProperty("m_Components");
            m_WeavedAssembliesList = new ReorderableList(serializedObject, m_WeavedAssemblies);
            m_WeavedAssembliesList.drawElementCallback += OnWeavedAssemblyDrawElement;
            m_WeavedAssembliesList.onAddCallback += OnWeavedAssemblyElementAdded;
            m_WeavedAssembliesList.drawHeaderCallback += OnWeavedAssemblyHeader;

            // Labels 
            m_WeavedAssemblyHeaderLabel = new GUIContent("Weaved Assemblies");
        }
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Weaver", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(m_Components);
            m_WeavedAssembliesList.DoLayoutList();
        }

        #region -= Weaved Assemblies =-
        private void OnWeavedAssemblyDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty indexProperty = m_WeavedAssemblies.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, indexProperty);
        }

        private void OnWeavedAssemblyElementAdded(ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            IList<Assembly> cachedAssemblies = AssemblyUtility.GetUserCachedAssemblies();

            for (int x = 0; x < cachedAssemblies.Count; x++)
            {
                bool foundMatch = false;
                for (int y = 0; y < m_WeavedAssemblies.arraySize; y++)
                {
                    SerializedProperty current = m_WeavedAssemblies.GetArrayElementAtIndex(y);
                    SerializedProperty assetPath = current.FindPropertyRelative("m_FilePath");
                    if (string.Equals(cachedAssemblies[x].Location, assetPath.stringValue, StringComparison.Ordinal))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    GUIContent content = new GUIContent(cachedAssemblies[x].GetName().Name);
                    menu.AddItem(content, false, OnWeavedAssemblyAdded, cachedAssemblies[x].Location);
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
    }
}

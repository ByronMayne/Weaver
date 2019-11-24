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
        public class Styles
        {
            public GUIStyle zebraStyle;
            public GUIContent cachedContent;

            public Styles()
            {
                cachedContent = new GUIContent();

                Texture2D altTexutre = new Texture2D(1, 1);
                altTexutre.SetPixel(0, 0, new Color32(126, 126, 126, 50));
                altTexutre.Apply();

                Texture2D selectedTexture = new Texture2D(1, 1);
                selectedTexture.SetPixel(0, 0, new Color32(0, 140, 255, 40));
                selectedTexture.Apply();

                zebraStyle = new GUIStyle(GUI.skin.label)
                {
                    onHover = { background = altTexutre },
                    onFocused = { background = selectedTexture }
                };
                // Set Color 
                Color zebraFontColor = zebraStyle.normal.textColor;
                zebraStyle.onFocused.textColor = zebraFontColor;
                zebraStyle.onHover.textColor = zebraFontColor;

                // Set Height
                zebraStyle.fixedHeight = 20;
                zebraStyle.alignment = TextAnchor.MiddleLeft;

                zebraStyle.richText = true;
            }

            public GUIContent Content(string message)
            {
                cachedContent.text = message;
                return cachedContent;
            }
        }

        // Properties
        private SerializedProperty m_WeavedAssemblies;
        private SerializedProperty m_Components;
        private SerializedProperty m_Enabled;
        private SerializedProperty m_IsSymbolsDefined;
        private SerializedProperty m_RequiredScriptingSymbols;
        private Log m_Log;

        // Lists
        private ReorderableList m_WeavedAssembliesList;

        // Layouts
        private Vector2 m_LogScrollPosition;
        private int m_SelectedLogIndex;

        // Labels
        private GUIContent m_WeavedAssemblyHeaderLabel;
        private static Styles m_Styles;

        private bool _hasModifiedProperties;

        private void OnEnable()
        {
            AssemblyUtility.PopulateAssemblyCache();
            m_WeavedAssemblies = serializedObject.FindProperty("m_WeavedAssemblies");
            m_Components = serializedObject.FindProperty("m_Components");
            m_Enabled = serializedObject.FindProperty("m_IsEnabled");

            // Get the log
            m_Log = serializedObject.FindField<Log>("m_Log").value;

            m_RequiredScriptingSymbols = serializedObject.FindProperty("m_RequiredScriptingSymbols");
            m_IsSymbolsDefined = m_RequiredScriptingSymbols.FindPropertyRelative("m_IsActive");
            m_WeavedAssembliesList = new ReorderableList(serializedObject, m_WeavedAssemblies);
            m_WeavedAssembliesList.drawHeaderCallback += OnWeavedAssemblyDrawHeader;
            m_WeavedAssembliesList.drawElementCallback += OnWeavedAssemblyDrawElement;
            m_WeavedAssembliesList.onAddCallback += OnWeavedAssemblyElementAdded;
            m_WeavedAssembliesList.drawHeaderCallback += OnWeavedAssemblyHeader;
            m_WeavedAssembliesList.onRemoveCallback += OnWeavedAssemblyRemoved;
            // Labels 
            m_WeavedAssemblyHeaderLabel = new GUIContent("Weaved Assemblies");
        }

        private void OnDisable()
        {
            if (_hasModifiedProperties)
            {
                string title = "Weaver Settings Pending Changes";
                string message = "You currently have some pending changes that have not been applied and will be lost. Would you like to apply them now?";
                string ok = "Apply Changes";
                string cancel = "Discard Changes";
                bool shouldApply = EditorUtility.DisplayDialog(title, message, ok, cancel);
                if (shouldApply)
                {
                    ApplyModifiedProperties();
                }
                _hasModifiedProperties = false;
            }
        }

        private void OnWeavedAssemblyDrawHeader(Rect rect)
        {
            GUI.Label(rect, WeaverContent.settingsWeavedAsesmbliesTitle);
        }

        private void OnWeavedAssemblyRemoved(ReorderableList list)
        {
            m_WeavedAssemblies.DeleteArrayElementAtIndex(list.index);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                if (m_Styles == null)
                {
                    m_Styles = new Styles();
                }

                GUILayout.Label("Settings", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(m_Enabled);
                EditorGUILayout.PropertyField(m_RequiredScriptingSymbols);

                if (!m_Enabled.boolValue)
                {
                    EditorGUILayout.HelpBox("Weaver will not run as it's currently disabled.", MessageType.Info);
                }
                else if (!m_IsSymbolsDefined.boolValue)
                {
                    EditorGUILayout.HelpBox("Weaver will not run the required scripting symbols are not defined.", MessageType.Info);
                }
                GUILayout.Box(GUIContent.none, GUILayout.Height(3f), GUILayout.ExpandWidth(true));

                EditorGUILayout.PropertyField(m_Components);
                m_WeavedAssembliesList.DoLayoutList();
            }
            if (EditorGUI.EndChangeCheck())
            {
                _hasModifiedProperties = true;
            }
            GUILayout.Label("Log", EditorStyles.boldLabel);
            DrawLogs();

            if (_hasModifiedProperties)
            {
                if (GUILayout.Button("Apply Modified Properties"))
                {
                    ApplyModifiedProperties();
                }
            }
        }

        private void ApplyModifiedProperties()
        {
            _hasModifiedProperties = false;
            serializedObject.ApplyModifiedProperties();
            AssemblyUtility.DirtyAllScripts();
            serializedObject.Update();
        }

        private void DrawLogs()
        {
            m_LogScrollPosition = EditorGUILayout.BeginScrollView(m_LogScrollPosition, EditorStyles.textArea);
            {
                for (int i = 0; i < m_Log.entries.Count; i++)
                {
                    Log.Entry entry = m_Log.entries[i];
                    if (m_Styles == null)
                    {
                        m_Styles = new Styles();
                    }

                    Rect position = GUILayoutUtility.GetRect(m_Styles.Content(entry.message), m_Styles.zebraStyle);
                    // Input
                    int controlID = GUIUtility.GetControlID(321324, FocusType.Keyboard, position);
                    Event current = Event.current;
                    EventType eventType = current.GetTypeForControl(controlID);
                    if (eventType == EventType.MouseDown && position.Contains(current.mousePosition))
                    {
                        if (current.clickCount == 2)
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            InternalEditorUtility.OpenFileAtLineExternal(entry.fileName, entry.lineNumber);
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                        GUIUtility.keyboardControl = controlID;
                        m_SelectedLogIndex = i;
                        current.Use();
                        GUI.changed = true;
                    }

                    if (current.type == EventType.KeyDown)
                    {
                        if (current.keyCode == KeyCode.UpArrow && m_SelectedLogIndex > 0)
                        {
                            m_SelectedLogIndex--;
                            current.Use();
                        }

                        if (current.keyCode == KeyCode.DownArrow && m_SelectedLogIndex < m_Log.entries.Count - 1)
                        {
                            m_SelectedLogIndex++;
                            current.Use();
                        }
                    }


                    if (eventType == EventType.Repaint)
                    {
                        bool isHover = entry.id % 2 == 0;
                        bool isActive = false;
                        bool isOn = true;
                        bool hasKeyboardFocus = m_SelectedLogIndex == i;
                        m_Styles.zebraStyle.Draw(position, m_Styles.Content(entry.message), isHover, isActive, isOn, hasKeyboardFocus);
                    }
                }

                if (m_SelectedLogIndex < 0 || m_SelectedLogIndex >= m_Log.entries.Count)
                {
                    // If we go out of bounds we zero out our selection
                    m_SelectedLogIndex = -1;
                }
            }
            EditorGUILayout.EndScrollView();
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
                    SerializedProperty assetPath = current.FindPropertyRelative("m_RelativePath");
                    if (cachedAssemblies[x].Location.IndexOf(assetPath.stringValue, StringComparison.Ordinal) > 0)
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    GUIContent content = new GUIContent(cachedAssemblies[x].GetName().Name);
                    string projectPath = FileUtility.SystemToProjectPath(cachedAssemblies[x].Location);
                    menu.AddItem(content, false, OnWeavedAssemblyAdded, projectPath);
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
            weaved.FindPropertyRelative("m_RelativePath").stringValue = (string)path;
            weaved.FindPropertyRelative("m_IsActive").boolValue = true;
        }
        #endregion
    }
}
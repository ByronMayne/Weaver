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
                GUIStyle altStyle = "LODSliderRange";
                GUIStyle selectedStyle = "MeTransitionSelect";
                cachedContent = new GUIContent();
                zebraStyle = new GUIStyle(GUI.skin.label)
                {
                    onHover = { background = altStyle.normal.background },
                    onFocused = { background = selectedStyle.normal.background }
                };
                zebraStyle.onFocused.textColor = zebraStyle.normal.textColor;
                zebraStyle.border = selectedStyle.border;
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
        private SerializedProperty m_Log;
        private SerializedProperty m_Entries;
        private SerializedProperty m_RequiredScriptingSymbols;

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
            m_Log = serializedObject.FindProperty("m_Log");
            m_Entries = m_Log.FindPropertyRelative("m_Entries");
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
                CGWrapper.Begin("Docs/WeaverFull.png");
                {
                    if (m_Styles == null)
                    {
                        m_Styles = new Styles();
                    }

                    GUILayout.Label("Settings", EditorStyles.boldLabel);

                    CGWrapper.Begin("Docs/EnabledSettings.png");
                    {
                        EditorGUILayout.PropertyField(m_Enabled);
                        EditorGUILayout.PropertyField(m_RequiredScriptingSymbols);
                    }
                    CGWrapper.End();

                    if (!m_Enabled.boolValue)
                    {
                        EditorGUILayout.HelpBox("Weaver will not run as it's currently disabled.", MessageType.Info);
                    }
                    else if (!m_IsSymbolsDefined.boolValue)
                    {
                        EditorGUILayout.HelpBox("Weaver will not run the required scripting symbols are not defined.", MessageType.Info);
                    }
                    GUILayout.Box(GUIContent.none, GUILayout.Height(3f), GUILayout.ExpandWidth(true));


                    CGWrapper.Begin("Docs/WeaverComponents.png");
                    {
                        EditorGUILayout.PropertyField(m_Components);
                    }
                    CGWrapper.End();
                    CGWrapper.Begin("Docs/WeavedAssemblies.png");
                    {
                        m_WeavedAssembliesList.DoLayoutList();
                    }
                    CGWrapper.End();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _hasModifiedProperties = true;
                }
                CGWrapper.Begin("Docs/Logs.png");
                {
                    GUILayout.Label("Log", EditorStyles.boldLabel);
                    DrawLogs();
                }
                CGWrapper.End();
            }
            CGWrapper.End();

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
                for (int i = 0; i < m_Entries.arraySize; i++)
                {
                    SerializedProperty entry = m_Entries.GetArrayElementAtIndex(i);
                    if (m_Styles == null)
                    {
                        m_Styles = new Styles();
                    }

                    SerializedProperty message = entry.FindPropertyRelative("message");
                    SerializedProperty id = entry.FindPropertyRelative("id");
                    Rect position = GUILayoutUtility.GetRect(m_Styles.Content(message.stringValue), m_Styles.zebraStyle);
                    // Input
                    int controlID = GUIUtility.GetControlID(321324, FocusType.Keyboard, position);
                    Event current = Event.current;
                    EventType eventType = current.GetTypeForControl(controlID);
                    if (eventType == EventType.MouseDown && position.Contains(current.mousePosition))
                    {
                        if (current.clickCount == 2)
                        {
                            SerializedProperty fileName = entry.FindPropertyRelative("fileName");
                            SerializedProperty lineNumber = entry.FindPropertyRelative("lineNumber");
                            InternalEditorUtility.OpenFileAtLineExternal(fileName.stringValue, lineNumber.intValue);
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

                        if (current.keyCode == KeyCode.DownArrow && m_SelectedLogIndex < m_Entries.arraySize - 1)
                        {
                            m_SelectedLogIndex++;
                            current.Use();
                        }
                    }


                    if (eventType == EventType.Repaint)
                    {
                        bool isHover = id.intValue % 2 == 0;
                        bool isActive = false;
                        bool isOn = true;
                        bool hasKeyboardFocus = m_SelectedLogIndex == i;
                        m_Styles.zebraStyle.Draw(position, m_Styles.Content(message.stringValue), isHover, isActive, isOn, hasKeyboardFocus);
                    }
                }
                GUILayout.FlexibleSpace();

                if (m_SelectedLogIndex < 0 || m_SelectedLogIndex >= m_Entries.arraySize)
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
            weaved.FindPropertyRelative("m_Enabled").boolValue = true;
        }
        #endregion

#if CAPTURE_GROUPS
        [MenuItem("CONTEXT/WeaverSettings/Toggle Debug Capture Groups")]
        public static void CaptureGroupsToggleDebugMenu(MenuCommand command)
        {
            CaptureGroup.ShowDebug = !CaptureGroup.ShowDebug;
        }

        [MenuItem("CONTEXT/WeaverSettings/Preform Capture")]
        public static void CaptureGroupPreformCapture(MenuCommand command)
        {
            WeaverSettingsEditor[] inspectors = Resources.FindObjectsOfTypeAll<WeaverSettingsEditor>();
            CaptureGroup.CapturePadding = new RectOffset(4, 4, 4, 4);
            if (inspectors.Length > 0)
            {
                CaptureGroup.PreformCapture(inspectors[0]);
            }
        }
#endif
    }
}
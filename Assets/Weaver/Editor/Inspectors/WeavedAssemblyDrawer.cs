using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Weaver.Editors
{
    [CustomPropertyDrawer(typeof(WeavedAssembly))]
    public class WeavedAssemblyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            {
                const float BUTTON_WIDTH = 25;
                SerializedProperty relativePath = property.FindPropertyRelative("m_RelativePath");
                SerializedProperty isActive = property.FindPropertyRelative("m_IsActive");
                position.width -= BUTTON_WIDTH;
                EditorGUI.LabelField(position, relativePath.stringValue, EditorStyles.textArea);
                position.x += position.width;
                position.width = BUTTON_WIDTH;
                isActive.boolValue = EditorGUI.Toggle(position, isActive.boolValue);
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

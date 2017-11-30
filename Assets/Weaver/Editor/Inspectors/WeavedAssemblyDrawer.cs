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
        // Property drawer instance are shared for arrays so we store this cache so we don't slow down the editor.
        private IDictionary<string, ReflectedField<bool>> _isValidCache = new Dictionary<string, ReflectedField<bool>>();
        private GUIContent m_MissingAssemblyLabel = new GUIContent("Missing Assembly: Please Remove");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReflectedField<bool> isValid = null;

            if(!_isValidCache.ContainsKey(property.propertyPath))
            {
                property.FindMethodRelative("OnValidate").Invoke();
                isValid = property.FindField<bool>("m_IsValid");
                _isValidCache[property.propertyPath] = isValid;
            }
            else
            {
                isValid = _isValidCache[property.propertyPath];
            }

            EditorGUI.BeginChangeCheck();
            {
                const float BUTTON_WIDTH = 25;
                SerializedProperty relativePath = property.FindPropertyRelative("m_RelativePath");
                SerializedProperty enabled = property.FindPropertyRelative("m_Enabled");

                if (!isValid.value)
                {
                    GUI.Box(position, m_MissingAssemblyLabel);
                }

                position.width -= BUTTON_WIDTH;
                EditorGUI.SelectableLabel(position, relativePath.stringValue, EditorStyles.textArea);
                position.x += position.width;
                position.width = BUTTON_WIDTH;
                enabled.boolValue = EditorGUI.Toggle(position, enabled.boolValue);
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

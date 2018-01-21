using UnityEditor;
using UnityEngine;

namespace Weaver
{
    [CustomPropertyDrawer(typeof(ScriptingSymbols))]
    public class ScriptingSymbolsEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            GUI.Label(position, label);

            position.y += EditorGUIUtility.singleLineHeight;

            // Get the value field
            property = property.FindPropertyRelative("value");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.DelayedTextField(position, property, GUIContent.none);
            }
            if (EditorGUI.EndChangeCheck())
            {
                // Filter out input
                string value = property.stringValue;
                if (!string.IsNullOrEmpty(value))
                {
                    char[] result = new char[value.Length];
                    int length = 0;
                    for (int i = 0; i < value.Length; i++)
                    {
                        char letter = value[i];

                        if ((letter >= 'a' && letter <= 'z') ||
                            (letter >= 'A' && letter <= 'Z'))
                        {
                            result[length] = letter;
                            length++;
                            continue;
                        }

                        if (letter == '!' && (i == 0 || value[i - 1] == ';'))
                        {
                            result[length] = letter;
                            length++;
                        }

                        switch (letter)
                        {
                            case '_':
                                result[length] = letter;
                                length++;
                                break;
                            case ';':
                                // Don't allow double semi colons.
                                if (length > 0 && result[length - 1] != ';')
                                {
                                    result[length] = letter;
                                    length++;
                                }
                                break;
                            case ' ':
                                result[length] = ';';
                                length++;
                                break;
                        }
                    }
                    property.stringValue = new string(result, 0, length);
                }
            }
        }
    }
}

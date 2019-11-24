using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Weaver.Editors
{
    [CustomPropertyDrawer(typeof(ComponentController))]
    public class ComponentControllerDrawer : PropertyDrawer
    {
        private SerializedProperty m_SubObjects;
        private ReflectedMethod m_AddItemMethod;
        private ReflectedMethod m_RemoveItemMethod;
        private ReflectedMethod m_HasInstanceOfTypeMethod;
        private ReorderableList m_ReoderableList;
        private bool m_Initialized = false;
        private Rect m_Position;
        private float m_Height;

        private void Initialize(SerializedProperty property)
        {
            if (!m_Initialized)
            {
                m_Initialized = true;
                m_SubObjects = property.FindPropertyRelative("m_SubObjects");
                m_AddItemMethod = property.FindMethodRelative("Add", typeof(Type));
                m_RemoveItemMethod = property.FindMethodRelative("Remove", typeof(int));
                m_HasInstanceOfTypeMethod = property.FindMethodRelative("HasInstanceOfType", typeof(Type));


                m_ReoderableList = new ReorderableList(m_SubObjects.serializedObject, m_SubObjects) { draggable = true };
                m_ReoderableList.onAddCallback += OnComponentAdded;
                m_ReoderableList.onRemoveCallback += OnComponentRemoved;
                m_ReoderableList.drawHeaderCallback += OnDrawHeader;
                m_ReoderableList.drawElementCallback += OnDrawElement;
            }
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2.0f;
            SerializedProperty element = m_SubObjects.GetArrayElementAtIndex(index);
            SerializedObject serializedObject = new SerializedObject(element.objectReferenceValue);
            rect.width -= 20f;
            GUI.Label(rect, element.objectReferenceValue.name, EditorStyles.textArea);
            rect.x += rect.width;
            rect.width = 20f;
            SerializedProperty isEnabled = serializedObject.FindProperty("m_IsActive");
            isEnabled.boolValue = EditorGUI.Toggle(rect, isEnabled.boolValue);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDrawHeader(Rect rect)
        {
            GUI.Label(rect, WeaverContent.settingsComponentsTitle);
        }

        private void OnComponentRemoved(ReorderableList list)
        {
            Object removedObject = m_SubObjects.GetArrayElementAtIndex(list.index).objectReferenceValue;
            if (removedObject != null)
            {
                string removedElementType = removedObject.GetType().FullName;
            }
            m_RemoveItemMethod.Invoke(list.index);
            OnComponentAddedOrRemoved();
        }

        private void OnComponentAdded(ReorderableList list)
        {
            // Create the generic menu
            GenericMenu componentMenu = new GenericMenu();
            // Get all the types that inherit from Weaver Component 
            IList<Type> componentTypes = AssemblyUtility.GetInheirtingTypesFromUserAssemblies<WeaverComponent>();
            // Loop over them all
            for (int i = 0; i < componentTypes.Count; i++)
            {
                Type type = componentTypes[i];
                // Check if we already have that type
                if (m_HasInstanceOfTypeMethod.Invoke(type).AreEqual(false))
                {
                    GUIContent menuLabel = new GUIContent(type.Assembly.GetName().Name + "/" + type.Name);
                    componentMenu.AddItem(menuLabel, false, OnTypeAdded, type);
                }
            }

            if (componentMenu.GetItemCount() == 0)
            {
                componentMenu.AddDisabledItem(new GUIContent("[All Components Added]"));
            }

            // We are just trying to align the menu to the plus box.
            Rect menuDisplayRect = m_Position;
            menuDisplayRect.height = EditorGUIUtility.singleLineHeight;
            menuDisplayRect.y += m_Position.height - EditorGUIUtility.singleLineHeight;
            menuDisplayRect.x += EditorGUIUtility.currentViewWidth - 100;
            componentMenu.DropDown(menuDisplayRect);
        }

        private void OnTypeAdded(object argument)
        {
            m_AddItemMethod.Invoke(argument);
            OnComponentAddedOrRemoved();
        }

        private void OnComponentAddedOrRemoved()
        {
            SerializedObject serializedObject = m_SubObjects.serializedObject;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            m_Height = m_ReoderableList.GetHeight();
            return m_Height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            m_Position = position;
            m_ReoderableList.DoList(position);
        }
    }
}
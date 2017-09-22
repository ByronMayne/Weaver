using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Weaver.Editors
{
    [CustomPropertyDrawer(typeof(ComponentController))]
    public class ComponentControllerEditor : PropertyDrawer
    {
        private GUIContent m_HeaderLabel = new GUIContent("Components");
        private SerializedProperty m_SubObjects;
        private SerializedMethod m_AddItemMethod;
        private SerializedMethod m_RemoveItemMethod;
        private SerializedMethod m_HasInstanceOfTypeMethod;
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


                m_ReoderableList = new ReorderableList(m_SubObjects.serializedObject, m_SubObjects);
                m_ReoderableList.draggable = true;
                m_ReoderableList.onAddCallback += OnComponentAdded;
                m_ReoderableList.onRemoveCallback += OnComponentRemoved;
                m_ReoderableList.drawHeaderCallback += OnDrawHeader;
                m_ReoderableList.drawElementCallback += OnDrawElement;
            }
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 4.0f;
            EditorGUI.PropertyField(rect, m_SubObjects.GetArrayElementAtIndex(index), GUIContent.none);
        }

        private void OnDrawHeader(Rect rect)
        {
            GUI.Label(rect, m_HeaderLabel);
        }

        private void OnComponentRemoved(ReorderableList list)
        {
            m_RemoveItemMethod.Invoke(list.index);
            m_SubObjects.serializedObject.Update();
        }

        private void OnComponentAdded(ReorderableList list)
        {
            // Create the generic menu
            GenericMenu componentMenu = new GenericMenu();
            // Get all the types that inheirt from Weaver Component 
            IList<Type> componentTypes = AssemblyUtility.GetInheirtingTypesFromUserAssemblies<WeaverComponent>();
            // Loop over them all
            for(int i = 0; i < componentTypes.Count; i++)
            {
                Type type = componentTypes[i];
                // Check if we already have that type
                if(!(bool)m_HasInstanceOfTypeMethod.Invoke(type))
                {
                    GUIContent menuLabel = new GUIContent(type.Assembly.GetName().Name + "/" + type.Name);
                    GenericMenu.MenuFunction menuFunction = () => 
                        {
                            m_AddItemMethod.Invoke(type);
                            m_SubObjects.serializedObject.Update();
                        };
                    componentMenu.AddItem(menuLabel, false, menuFunction);
                }
            }
            
            if(componentMenu.GetItemCount() == 0)
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
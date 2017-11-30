using System;
using System.Reflection;
using UnityEditor;

/// <summary>
/// The same design as a serialized property but a target for a method.
/// </summary
public class ReflectedMethod
{
    private string m_MethodPath;
    private SerializedObject m_SerializedObject;
    private MethodInfo m_MethodInfo;

    public ReflectedMethod(SerializedObject serializedObject, string methodPath)
    {
        m_SerializedObject = serializedObject;
        m_MethodPath = methodPath;
    }

    public object Invoke(params object[] arguments)
    {
        Type[] parameters = new Type[arguments.Length];
        for(int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = arguments[i].GetType();
        }

        object instance = null;

        for(int i = 0; i < m_SerializedObject.targetObjects.Length; i++)
        {
            instance = m_SerializedObject.targetObjects[i];
            string[] members = m_MethodPath.Split('.');

            for(int memberIndex = 0; memberIndex < members.Length; memberIndex++)
            {
                string memberName = members[memberIndex];
                Type instanceType = instance.GetType();

                if(memberIndex == members.Length - 1)
                {
                    m_MethodInfo = instanceType.GetMethod(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, parameters, null);
                }
                else
                {
                    FieldInfo fieldInfo = instanceType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    instance = fieldInfo.GetValue(instance);
                }
            }

            if (m_MethodInfo.IsStatic)
            {
                instance = null;
                // Only invoke static methods once. 
                break;
            }
        }

        // Invoke
        return m_MethodInfo.Invoke(instance, arguments);
    }
}
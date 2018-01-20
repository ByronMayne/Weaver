using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

public struct ReturnValue
{
    private object[] m_Values;
    private bool m_HasMixedValues;

    public ReturnValue(object[] values)
    {
        m_HasMixedValues = false;
        m_Values = new object[values.Length];
        for (int x = 0; x < values.Length; x++)
        {
            m_Values[x] = values[x];
            for (int y = 0; y < values.Length; y++)
            {
                if (x != y)
                {
                    m_HasMixedValues = true;
                }
            }
        }
    }

    public bool AreEqual<T>(T value)
    {
        if (m_HasMixedValues)
        {
            return false;
        }
        for (int i = 0; i < m_Values.Length; i++)
        {
            if(value == null && m_Values[i] != null)
            {
                return false; 
            }

            if(m_Values[i] == null && value != null)
            {
                return false;
            }

            if(!Equals(m_Values[i], value))
            {
                return false;
            }
        }
        return true;
    }
}

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

    public ReturnValue Invoke(params object[] arguments)
    {
        Type[] parameters = new Type[arguments.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = arguments[i].GetType();
        }
        object[] results = new object[m_SerializedObject.targetObjects.Length];
        object instance = null;

        for (int i = 0; i < m_SerializedObject.targetObjects.Length; i++)
        {
            instance = m_SerializedObject.targetObjects[i];
            string[] members = m_MethodPath.Split('.');

            for (int memberIndex = 0; memberIndex < members.Length; memberIndex++)
            {
                string memberName = members[memberIndex];
                Type instanceType = instance.GetType();

                if (string.CompareOrdinal("Array", memberName) == 0)
                {
                    // Skip to the next index
                    memberIndex++;
                    // Array.data[0] // Example of what we are trying to parse 
                    string arrayPath = members[memberIndex];
                    // grab our index
                    int arrayIndex = ReflectedMembers.GetArrayIndexFromPropertyPath(arrayPath);
                    // Cast our instance as a IList
                    IList asList = (IList)instance;
                    // Grab the element
                    instance = asList[arrayIndex];
                }
                else if (memberIndex == members.Length - 1)
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
                // Invoke the method
                return new ReturnValue(new object[] { m_MethodInfo.Invoke(instance, arguments) });
            }
            // Invoke the method
            results[i] = m_MethodInfo.Invoke(instance, arguments);
        }
        return new ReturnValue(results);
    }
}
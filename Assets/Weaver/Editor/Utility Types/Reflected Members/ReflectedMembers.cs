using UnityEngine;
using UnityEditor;
using System;

public static class ReflectedMembers
{
    public static ReflectedMethod FindMethod(this SerializedObject serializedObject, string methodName)
    {
        return new ReflectedMethod(serializedObject, methodName);
    }

    public static ReflectedMethod FindMethod(this SerializedObject serializedObject, string methodName, params Type[] parameters)
    {
        return new ReflectedMethod(serializedObject, methodName);
    }

    public static ReflectedMethod FindMethodRelative(this SerializedProperty serializedProperty, string methodName)
    {
        string methodPath = serializedProperty.propertyPath + "." + methodName;
        return new ReflectedMethod(serializedProperty.serializedObject, methodPath);
    }

    public static ReflectedMethod FindMethodRelative(this SerializedProperty serializedProperty, string methodName, params Type[] parameters)
    {
        string methodPath = serializedProperty.propertyPath + "." + methodName;
        return new ReflectedMethod(serializedProperty.serializedObject, methodPath);
    }

    public static ReflectedField<T> FindField<T>(this SerializedObject serializedObject, string methodName)
    {
        return new ReflectedField<T>(serializedObject, methodName);
    }

    public static ReflectedField<T> FindField<T>(this SerializedProperty serializedProperty, string methodName)
    {
        string methodPath = serializedProperty.propertyPath + "." + methodName;
        return new ReflectedField<T>(serializedProperty.serializedObject, methodPath);
    }

    public static int GetArrayIndexFromPropertyPath(string propertyPath)
    {
        int pathLength = propertyPath.Length - 2;
        int i = pathLength;

        while (i >= 0)
        {
            i--;
            if (!char.IsDigit(propertyPath[i]))
            {
                break;
            }
        }
        int length = pathLength - i;
        int startIndex = propertyPath.Length - (propertyPath.Length - i) + 1;
        string digits = propertyPath.Substring(startIndex, length);
        return int.Parse(digits);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;
using Mono.Collections.Generic;

public static class ConstructorArguments
{
    public static T GetValue<T>(this CustomAttribute customAttribute, string propertyName)
    {
        for (int i = 0; i < customAttribute.Properties.Count; i++)
        {
            CustomAttributeNamedArgument arguement = customAttribute.Properties[i];

            if(string.Equals(propertyName, arguement.Name, System.StringComparison.Ordinal))
            {
                return (T)arguement.Argument.Value;
            }

        }
        return default(T);
    }
}

using Mono.Cecil;
using Mono.Collections.Generic;
using System;

namespace Weaver.Extensions
{
    public static class ICustomAttributeProviderExtensions
    {
        public static bool HasCustomAttribute<T>(this ICustomAttributeProvider instance)
        {
            if (!instance.HasCustomAttributes) return false;

            Collection<CustomAttribute> attributes = instance.CustomAttributes;

            for(int i = 0;  i < attributes.Count; i++)
            {
                if(attributes[i].AttributeType.FullName.Equals(typeof(T).FullName, StringComparison.Ordinal))
                {
                    return true; 
                }
            }
            return false;
        }

        public static CustomAttribute GetCustomAttribute<T>(this ICustomAttributeProvider instance)
        {
            if (!instance.HasCustomAttributes) return null;

            Collection<CustomAttribute> attributes = instance.CustomAttributes;

            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].AttributeType.FullName.Equals(typeof(T).FullName, StringComparison.Ordinal))
                {
                    return attributes[i];
                }
            }
            return null;
        }
    }
}

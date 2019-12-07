using Mono.Cecil;
using Mono.Collections.Generic;
using System;

namespace Weaver
{
    /// <summary>
    /// Contains extension methods for working with <see cref="ICustomAttributeProvider"/>
    /// </summary>
    public static class CustomAttributeProviderExtensions
    {
        /// <summary>
        /// Determines whether this instance has the specified attribute.
        /// </summary>
        /// <typeparam name="T">The type of attribute</typeparam>
        /// <param name="provider">The provider.</param>
        /// <returns>
        ///   <c>true</c> if the specified provider has attribute; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            if (!provider.HasCustomAttributes) return false;
            Collection<CustomAttribute> attributes = provider.CustomAttributes;

            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].AttributeType.FullName.Equals(typeof(T).FullName, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a custom attribute from a provided and returns it if it exists otherwise returns null.
        /// </summary>
        /// <typeparam name="T">The attribute you are looking for</typeparam>
        /// <param name="provider">The provider.</param>
        /// <returns>The custom attribute or null if it does not exist</returns>
        public static CustomAttribute GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            if (!provider.HasCustomAttributes) return null;

            Collection<CustomAttribute> attributes = provider.CustomAttributes;

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

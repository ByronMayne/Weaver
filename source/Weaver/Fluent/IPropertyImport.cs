using Mono.Cecil;

namespace Weaver.Fluent
{
    public interface IPropertyImport<T>
    {
        /// <summary>
        /// Gets the type of that declairs this property
        /// </summary>
        ITypeImport<T> DeclaringType { get; }

        /// <summary>
        /// Gets the getter for a property if it exists otherwise it returns null
        /// </summary>
        /// <param name="methodReference">The method reference for the setter.</param>
        IPropertyImport<T> GetGetter(out MethodDefinition methodDefinition);


        /// <summary>
        /// Gets the getter for a property if it exists otherwise it returns null
        /// </summary>
        /// <param name="methodReference">The method reference for the setter.</param>
        IPropertyImport<T> GetGetter(out MethodReference methodReference);

        /// <summary>
        /// Gets the setter for a property if it exists otherwise it returns null
        /// </summary>
        /// <param name="methodReference">The method reference for the setter.</param>
        IPropertyImport<T> GetSetter(out MethodDefinition methodDefinition);

        /// <summary>
        /// Gets the setter for a property if it exists otherwise it returns null
        /// </summary>
        /// <param name="methodReference">The method reference for the setter.</param>
        IPropertyImport<T> GetSetter(out MethodReference methodReference);
    }
}
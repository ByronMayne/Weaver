using Mono.Cecil;
using System;
using System.Linq.Expressions;

namespace Weaver.Fluent
{
    public interface ITypeImport<T>
    {
        /// <summary>
        /// Gets the type defintion for the type.
        /// </summary>
        ITypeImport<T> GetType(out TypeDefinition typeDefinition);

        /// <summary>
        /// Gets the type reference for the type.
        /// </summary>
        ITypeImport<T> GetType(out TypeReference typeReference);

        /// <summary>
        /// Returns back the default constructor if defined.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        ITypeImport<T> GetConstructor(out MethodDefinition constructorDefinition);

        /// <summary>
        /// Returns back the default constructor if defined.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        ITypeImport<T> GetConstructor(out MethodReference constructorReference);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The method.</param>
        ITypeImport<T> GetMethod(string methodName, out MethodDefinition methodDefinition);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The method.</param>
        ITypeImport<T> GetMethod(string methodName, out MethodReference methodReference);

        /// <summary>
        /// Given in a lamda expression this returns back the property 
        /// </summary>
        /// <param name="accessor">The accessor.</param>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        ITypeImport<T> GetMethod(Expression<Func<T, Action>> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Given in a lamda expression this returns back the property 
        /// </summary>
        /// <param name="accessor">The accessor.</param>
        /// <param name="methodDefinition">The method reference.</param>
        /// <returns></returns>
        ITypeImport<T> GetMethod(Expression<Action<T>> expression, out MethodReference methodReference);

        /// <summary>
        /// Given in a lamda expression this returns back the property 
        /// </summary>
        /// <param name="accessor">The accessor.</param>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        ITypeImport<T> GetMethod(Expression<Action<T>> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Given in a lamda expression this returns back the property 
        /// </summary>
        /// <param name="accessor">The accessor.</param>
        /// <param name="methodDefinition">The method reference.</param>
        /// <returns></returns>
        ITypeImport<T> GetMethod(Expression<Func<T, Action>> expression, out MethodReference methodReference);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        IPropertyImport<T> GetProperty(string name);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        IPropertyImport<T> GetProperty(Expression<Func<T, object>> expression);

        /// <summary>
        ///  Given a lamda expression this will return a static function for the type
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        ITypeImport<T> GetStaticMethod(Expression<Func<Delegate>> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Given a lamda expression this will return a static function for the type
        /// </summary>
        ITypeImport<T> GetStaticMethod(Expression<Func<Delegate>> expression, out MethodReference methodReference);

        /// <summary>
        /// Given a lamda expression this will return a static function for the type
        /// </summary>
        ITypeImport<T> GetStaticMethod(Expression<Action> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Given a lamda expression this will return a static function for the type
        /// </summary>
        ITypeImport<T> GetStaticMethod(Expression<Action> expression, out MethodReference methodReference);

        /// <summary>
        /// Gets the definition for a static field.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <returns></returns>
        ITypeImport<T> GetStaticField(Expression<Func<object>> expression, out FieldDefinition fieldDefinition);

        /// <summary>
        /// Gets the reference for a static field.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="fieldReference">The field reference.</param>
        /// <returns></returns>
        ITypeImport<T> GetStaticField(Expression<Func<object>> expression, out FieldReference fieldReference);
    }
}

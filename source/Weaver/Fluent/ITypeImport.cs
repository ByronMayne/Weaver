using Mono.Cecil;
using System;
using System.Linq.Expressions;

namespace Weaver.Fluent
{

    /// <summary>
    /// Type import used for unknown types 
    /// </summary>
    public interface ITypeImport
    {
        /// <summary>
        /// Gets the type defintion for the type.
        /// </summary>
        ITypeImport GetType(out TypeDefinition typeDefinition);

        /// <summary>
        /// Gets the type reference for the type.
        /// </summary>
        ITypeImport GetType(out TypeReference typeReference);

        /// <summary>
        /// Returns back the default constructor if defined.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        ITypeImport GetConstructor(out MethodDefinition constructorDefinition);

        /// <summary>
        /// Returns back the default constructor if defined.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        ITypeImport GetConstructor(out MethodReference constructorReference);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The method.</param>
        ITypeImport GetMethod(string methodName, out MethodDefinition methodDefinition);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The method.</param>
        ITypeImport GetMethod(string methodName, out MethodReference methodReference);

        /// <summary>
        ///  Given a lamda expression this will return a static function for the type
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        ITypeImport GetStaticMethod(Expression<Func<Delegate>> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Given a lamda expression this will return a static function for the type
        /// </summary>
        ITypeImport GetStaticMethod(Expression<Func<Delegate>> expression, out MethodReference methodReference);

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="methodReference">The method reference.</param>
        /// <returns></returns>
        ITypeImport GetStaticMethod(Expression<Action> expression, out MethodReference methodReference);

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        ITypeImport GetStaticMethod(Expression<Action> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Gets the definition for a static field.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <returns></returns>
        ITypeImport GetStaticField(Expression<Func<object>> expression, out FieldDefinition fieldDefinition);

        /// <summary>
        /// Gets the reference for a static field.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="fieldReference">The field reference.</param>
        /// <returns></returns>
        ITypeImport GetStaticField(Expression<Func<object>> expression, out FieldReference fieldReference);
    }
}
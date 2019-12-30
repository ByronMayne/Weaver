using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Weaver.Fluent.Implementations;

namespace Weaver.Fluent
{
    public struct TypeImport<T> : ITypeImport<T>, ITypeImport
    {
        private readonly bool m_isExternalType;
        private readonly Type m_type;
        private readonly TypeDefinition m_typeDefinition;
        private readonly TypeReference m_typeReference;
        private readonly ModuleDefinition m_moduleDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeImport{T}"/> struct.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="type">The type.</param>
        public TypeImport(ModuleDefinition moduleDefinition, Type type)
        {
            m_type = type;
            m_moduleDefinition = moduleDefinition;
            m_typeDefinition = m_moduleDefinition.GetTypes()
                .Where(t => string.Equals(t.FullName, type.FullName))
                .FirstOrDefault();

            m_isExternalType = m_typeDefinition == null;

            if (m_isExternalType)
            {
                // Type is in another module so we have to import it and resolve it 
                m_typeReference = m_moduleDefinition.ImportReference(m_type);
                m_typeDefinition = m_typeReference.Resolve();
            }
            else
            {
                // It exists in the same module 
                m_typeReference = m_typeDefinition;
            }
        }

        /// <inheritdoc />
        public ITypeImport<T> GetType(out TypeDefinition typeDefinition)
        {
            typeDefinition = m_typeDefinition;
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetType(out TypeReference typeReference)
        {
            typeReference = m_typeReference;
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetConstructor(out MethodDefinition methodDefinition)
        {
            methodDefinition = m_typeDefinition.GetMethod(".ctor");
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetConstructor(out MethodReference methodReference)
        {
            GetConstructor(out MethodDefinition methodDefinition);

            if (m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodDefinition);
            }
            else
            {
                methodReference = methodDefinition;
            }

            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetMethod(string methodName, out MethodDefinition methodDefinition)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(nameof(methodName));

            methodDefinition = m_typeDefinition.GetMethod(methodName);

            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetMethod(string methodName, out MethodReference methodReference)
        {
            GetMethod(methodName, out MethodDefinition methodDefinition);

            if (m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodDefinition);
            }
            else
            {
                methodReference = methodDefinition;
            }
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetMethod(Expression<Func<T, Action>> expression, out MethodDefinition methodDefinition)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            UnaryExpression unary = expression.Body as UnaryExpression;

            if (unary == null)
                throw new FormatException("Expected Unary");

            MethodCallExpression methodCall = unary.Operand as MethodCallExpression;

            ConstantExpression constantExpression = methodCall.Object as ConstantExpression;

            MethodInfo method = constantExpression.Value as MethodInfo;


            methodDefinition = MethodQuery(method, m_typeDefinition)
                .Where(m => m.IsStatic)
                .FirstOrDefault();

            return GetMethod(method.Name, out methodDefinition);
        }

        /// <inheritdoc />
        public ITypeImport<T> GetMethod(Expression<Func<T, Action>> expression, out MethodReference methodReference)
        {
            GetMethod(expression, out MethodDefinition methodDefinition);

            if (m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodDefinition);
            }
            else
            {
                methodReference = methodDefinition;
            }
            return this;
        }

        /// <inheritdoc />
        public IPropertyImport<T> GetProperty(Expression<Func<T, object>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            UnaryExpression unary = expression.Body as UnaryExpression;

            if (unary == null)
                throw new FormatException("Expected Unary");

            MemberExpression memberExpression = unary.Operand as MemberExpression;

            string propertyName = memberExpression.Member.Name;
            return GetProperty(propertyName);
        }

        public IPropertyImport<T> GetProperty(string name)
        {
            return new PropertyImport<T>(this, m_moduleDefinition, name, m_isExternalType);
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticMethod(Expression<Action> expression, out MethodDefinition methodDefinition)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            MethodCallExpression methodCall = expression.Body as MethodCallExpression;
            MethodInfo method = methodCall.Method;

            methodDefinition = MethodQuery(method, m_typeDefinition)
                .Where(m => m.IsStatic)
                .FirstOrDefault();

            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticMethod(Expression<Action> expression, out MethodReference methodReference)
        {
            GetStaticMethod(expression, out MethodDefinition methodDefinition);

            if (m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodDefinition);
            }
            else
            {
                methodReference = methodDefinition;
            }
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetMethod(Expression<Action<T>> expression, out MethodReference methodReference)
        {
            GetMethod(expression, out MethodDefinition methodDefinition);
            methodReference = methodDefinition;
            if (m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodReference);
            }
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetMethod(Expression<Action<T>> expression, out MethodDefinition methodDefinition)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            MethodCallExpression methodCall = expression.Body as MethodCallExpression;

            GetMethod(methodCall.Method.Name, out methodDefinition);
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticMethod(Expression<Func<Delegate>> expression, out MethodDefinition methodDefinition)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            methodDefinition = null;

            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticMethod(Expression<Func<Delegate>> expression, out MethodReference methodReference)
        {
            GetStaticMethod(expression, out MethodDefinition methodDefinition);
            methodReference = methodDefinition;
            if(m_isExternalType)
            {
                methodReference = m_moduleDefinition.ImportReference(methodReference);
            }
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticField(Expression<Func<object>> expression, out FieldDefinition fieldDefinition)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            MemberExpression memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new FormatException("Expected a member expression line '() => ClassType.StaticProperty'");

            string propertyName = memberExpression.Member.Name;

            GetStaticField(propertyName, out fieldDefinition);

            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticField(string fieldName, out FieldDefinition fieldDefinition)
        {
            fieldDefinition = m_typeDefinition.GetField(a => string.Equals(a.Name, fieldName, StringComparison.Ordinal) && a.IsStatic);
            return this; 
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticField(string fieldName, out FieldReference fieldReference)
        {
            GetStaticField(fieldName, out FieldDefinition fieldDefinition);
            fieldReference = fieldDefinition;

            if (m_isExternalType)
            {
                fieldReference = m_moduleDefinition.ImportReference(fieldReference);
            }
            return this;
        }

        /// <inheritdoc />
        public ITypeImport<T> GetStaticField(Expression<Func<object>> expression, out FieldReference fieldReference)
        {
            GetStaticField(expression, out FieldDefinition fieldDefinition);
            fieldReference = fieldDefinition;
            if(m_isExternalType)
            {
                fieldReference = m_moduleDefinition.ImportReference(fieldReference);
            }
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetStaticMethod(Expression<Func<Delegate>> expression, out MethodDefinition methodDefinition)
        {
            GetStaticMethod(expression, out methodDefinition);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetStaticMethod(Expression<Func<Delegate>> expression, out MethodReference methodReference)
        {
            GetStaticMethod(expression, out methodReference);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetStaticField(Expression<Func<object>> expression, out FieldDefinition fieldDefinition)
        {
            GetStaticField(expression, out fieldDefinition);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetStaticField(Expression<Func<object>> expression, out FieldReference FieldReference)
        {
            GetStaticField(expression, out FieldReference);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetType(out TypeDefinition typeDefinition)
        {
            GetType(out typeDefinition);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetType(out TypeReference typeReference)
        {
            GetType(out typeReference);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetConstructor(out MethodDefinition constructorDefinition)
        {
            GetConstructor(out constructorDefinition);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetConstructor(out MethodReference constructorReference)
        {
            GetConstructor(out constructorReference);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetMethod(string methodName, out MethodDefinition methodDefinition)
        {
            GetMethod(methodName, out methodDefinition);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetMethod(string methodName, out MethodReference methodReference)
        {
            GetMethod(methodName, out methodReference);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetStaticMethod(Expression<Action> expression, out MethodReference methodReference)
        {
            GetStaticMethod(expression, out methodReference);
            return this;
        }

        /// <inheritdoc />
        ITypeImport ITypeImport.GetStaticMethod(Expression<Action> expression, out MethodDefinition methodDefinition)
        {
            GetStaticMethod(expression, out methodDefinition);
            return this;
        }

        private static bool ParametersMatch(IList<ParameterDefinition> lhs, IList<ParameterInfo> rhs)
        {
            if (lhs.Count != rhs.Count) return false;

            for (int i = 0; i < lhs.Count; i++)
            {
                if (!string.Equals(lhs[i].ParameterType.FullName, rhs[i].ParameterType.FullName, StringComparison.Ordinal))
                {
                    return false;
                }
            }
            return true;
        }

        private static IEnumerable<MethodDefinition> MethodQuery(MethodInfo method, TypeDefinition typeDefinition)
            => typeDefinition
                .Methods
                .Where(m => string.CompareOrdinal(m.Name, method.Name) == 0)
                .Where(m => ParametersMatch(m.Parameters, method.GetParameters()))
                .Where(m => string.CompareOrdinal(m.ReturnType.FullName, method.ReturnType.FullName) == 0);

 
    }
}

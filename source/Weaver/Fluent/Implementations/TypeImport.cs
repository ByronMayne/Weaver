using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Weaver.Fluent.Implementations;

namespace Weaver.Fluent
{
    public struct TypeImport<T> : ITypeImport<T>
    {
        private TypeDefinition m_typeDefinition;

        public TypeImport(ModuleDefinition moduleDefinition)
        {
            TypeReference typeReference = moduleDefinition.ImportReference(typeof(T));
            m_typeDefinition = typeReference.Resolve();
        }

        public ITypeImport<T> GetType(out TypeDefinition typeDefinition)
        {
            typeDefinition = m_typeDefinition;
            return this;
        }

        public ITypeImport<T> GetConstructor(out MethodDefinition constructorDef)
        {
            constructorDef = m_typeDefinition.GetMethod(".ctor");
            return this;
        }

        public ITypeImport<T> GetMethod(string methodName, out MethodDefinition methodDefinition)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(nameof(methodName));

            methodDefinition = m_typeDefinition.GetMethod(methodName);
            return this;
        }

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

        public IPropertyImport<T> GetProperty(Expression<Func<T, object>> property)
        {
            string propertyName = "";
            return GetProperty(propertyName);
        }

        public IPropertyImport<T> GetProperty(string name)
        {
            return new PropertyImport<T>(this);
        }

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

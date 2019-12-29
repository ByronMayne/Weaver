using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using Weaver.DataTypes;
using Weaver.Fluent;

namespace Weaver
{
    /// <summary>
    /// Contains extension methods for working with <see cref="TypeDefinition"/>
    /// </summary>
    public static class TypeDefinitionExtensions
    {
        public static ITypeImport<T> ImportFluent<T>(this ModuleDefinition module)
        {
            return new TypeImport<T>(module, typeof(T));
        }

        /// <summary>
        /// Used to import memebers for a static type. This can also be used for an unknown runtime type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="memberType">Type of the member.</param>
        /// <returns></returns>
        public static ITypeImport ImportFluent(this ModuleDefinition module, Type memberType)
        {
            return new TypeImport<object>(module, memberType);
        }

        /// <summary>
        /// Given a type this returns back the file it was declaired within and the postion and line number. 
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <returns></returns>
        public static MemberLocation GetLocation(this TypeDefinition typeDefinition)
        {
            for (int methodIndex = 0; methodIndex < typeDefinition.Methods.Count; methodIndex++)
            {
                if (typeDefinition.Methods[methodIndex].HasBody)
                {
                    MethodBody body = typeDefinition.Methods[methodIndex].Body;

                    for (int instructionIndex = 0; instructionIndex < body.Instructions.Count; instructionIndex++)
                    {
                        Instruction instruction = body.Instructions[instructionIndex];

                        SequencePoint sequencePoint = body.Method.DebugInformation.GetSequencePoint(instruction);

                        if (sequencePoint != null)
                        {
                            return new MemberLocation(sequencePoint.StartLine, sequencePoint.Offset, sequencePoint.Document.Url);
                        }
                    }
                }
            }
            return default(MemberLocation);
        }

        /// <summary>
        /// Finds a method from a type based on it's name. 
        /// </summary>
        public static MethodDefinition GetMethod(this TypeDefinition instance, string name)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDef = instance.Methods[i];

                if (string.CompareOrdinal(methodDef.Name, name) == 0)
                {
                    return methodDef;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the field based on it's name
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static FieldDefinition GetField(this TypeDefinition typeDefinition, string name)
        {
            for (int i = 0; i < typeDefinition.Fields.Count; i++)
            {
                FieldDefinition fieldDefinition = typeDefinition.Fields[i];
                if (string.CompareOrdinal(fieldDefinition.Name, name) == 0)
                {
                    return fieldDefinition;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the field based of a predeicate 
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static FieldDefinition GetField(this TypeDefinition typeDefinition, Predicate<FieldDefinition> filter)
        {
            for (int i = 0; i < typeDefinition.Fields.Count; i++)
            {
                FieldDefinition fieldDefinition = typeDefinition.Fields[i];
                if (filter(fieldDefinition))
                {
                    return fieldDefinition;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the method base of it's type and parameters it takes
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The method definition if found</returns>
        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, params Type[] parameterTypes)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDefinition = instance.Methods[i];

                if (!string.Equals(methodDefinition.Name, name, StringComparison.Ordinal) ||
                    parameterTypes.Length != methodDefinition.Parameters.Count)
                {
                    continue;
                }

                MethodDefinition result = methodDefinition;
                for (int x = methodDefinition.Parameters.Count - 1; x >= 0; x--)
                {
                    ParameterDefinition parameter = methodDefinition.Parameters[x];
                    if (!string.Equals(parameter.ParameterType.Name, parameterTypes[x].Name, StringComparison.Ordinal))
                    {
                        break;
                    }

                    if (x == 0)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the method based of a name and a list of type referneces 
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name of the method</param>
        /// <param name="parameterTypes">The parameter types it takes.</param>
        /// <returns>The definition if found</returns>
        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, params TypeReference[] parameterTypes)
        {
            if (instance.Methods != null)
            {
                for (int i = 0; i < instance.Methods.Count; i++)
                {
                    MethodDefinition methodDefinition = instance.Methods[i];
                    if (string.Equals(methodDefinition.Name, name, StringComparison.Ordinal) // Names Match
                        && parameterTypes.Length == methodDefinition.Parameters.Count) // The same number of parameters
                    {
                        MethodDefinition result = methodDefinition;
                        for (int x = methodDefinition.Parameters.Count - 1; x >= 0; x--)
                        {
                            ParameterDefinition parameter = methodDefinition.Parameters[x];
                            if (!string.Equals(parameter.ParameterType.Name, parameterTypes[x].Name, StringComparison.Ordinal))
                            {
                                break;
                            }

                            if (x == 0)
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a method from a type based on it's name and argument count
        /// </summary>
        public static MethodDefinition GetMethod(this TypeDefinition instance, string name, int argCount)
        {
            for (int i = 0; i < instance.Methods.Count; i++)
            {
                MethodDefinition methodDef = instance.Methods[i];

                if (string.CompareOrdinal(methodDef.Name, name) == 0 && methodDef.Parameters.Count == argCount)
                {
                    return methodDef;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the property from a type based off it's name. 
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static PropertyDefinition GetProperty(this TypeDefinition instance, string name)
        {
            for (int i = 0; i < instance.Properties.Count; i++)
            {
                PropertyDefinition preopertyDef = instance.Properties[i];

                // Properties can only have one argument or they are an indexer. 
                if (string.CompareOrdinal(preopertyDef.Name, name) == 0 && preopertyDef.Parameters.Count == 0)
                {
                    return preopertyDef;
                }
            }
            return null;
        }
    }
}

using Mono.Cecil;
using Mono.Cecil.Cil;
using Weaver.DataTypes;

namespace Weaver.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="MethodDefinition"/>s.
    /// </summary>
    public static class MethodDefinitionExtensions
    {
        /// <summary>
        /// Gets the location of a <see cref="MethodDefinition"/> using the provided debug symbols. If no
        /// symbols are provided this will just return the default type
        /// </summary>
        public static MemberLocation GetLocation(this MethodDefinition method)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                SequencePoint sequencePoint = method.DebugInformation.GetSequencePoint(instruction);

                if (sequencePoint != null)
                {
                    return new MemberLocation(sequencePoint.StartLine, sequencePoint.Offset, sequencePoint.Document.Url);
                }
            }
            return default(MemberLocation);
        }
    }
}

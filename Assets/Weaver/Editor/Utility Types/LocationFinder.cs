using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Weaver
{
    public class MemberLocation
    {
        public string Url { get; set; }

        public override string ToString()
        {
            return "[File" + Url + "]";
        }
    }

    public class MethodLocation : MemberLocation
    {
        public int Line { get; set; }

        public override string ToString()
        {
            return "[File: " + Url + ", Line: " + Line + "]";
        }
    }

    public static class LocationFinder
    {
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
                            return new MemberLocation
                            {
                                Url = sequencePoint.Document.Url
                            };
                        }
                    }
                }
            }
            return null;
        }


        public static MethodLocation GetLocation(this MethodDefinition method)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                SequencePoint sequencePoint = method.DebugInformation.GetSequencePoint(instruction);

                if (sequencePoint != null)
                {
                    return new MethodLocation
                    {
                        Url = sequencePoint.Document.Url,
                        Line = sequencePoint.StartLine
                    };
                }
            }
            return null;
        }
    }
}
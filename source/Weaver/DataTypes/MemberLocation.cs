
namespace Weaver.DataTypes
{
    /// <summary>
    /// A structure containg the line and position of a method in
    /// a file.
    /// </summary>
    public struct MemberLocation
    {
        /// <summary>
        /// The line number in the file
        /// </summary>
        public readonly int Line;

        /// <summary>
        /// The position in the file
        /// </summary>
        public readonly int Position;

        /// <summary>
        /// The name of the file it is in
        /// </summary>
        public readonly string File;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodLocation"/> struct.
        /// </summary>
        /// <param name="line">The line number in the file.</param>
        /// <param name="position">The position int he file.</param>
        /// <param name="file">The file.</param>
        public MemberLocation(int line, int position, string file)
        {
            Line = line;
            Position = position;
            File = file;
        }

        public override string ToString()
        {
            return $"{File}:{Line}:{Position}";
        }
    }
}

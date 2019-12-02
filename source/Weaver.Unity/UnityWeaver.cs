using System;
using Seed.IO;
using Weaver.Contracts;

namespace Weaver.Unity
{
    public class UnityWeaver
    {
        private readonly IAssemblyWeaver m_weaver;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWeaver"/> class. Created in
        /// the <see cref="EntryPoint"/> class. 
        /// </summary>
        public UnityWeaver()
        {
            // Create instance of weaver to work with. 
            m_weaver = new AssemblyWeaver(); 
        }




        internal void WeaveAssembly(AbsolutePath assemblyPath)
        {
            throw new NotImplementedException();
        }
    }
}

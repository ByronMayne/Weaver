using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Weaver.Tests.Fluent
{
    [TestClass]
    public class TypeImportTests
    {

        private ModuleDefinition m_moduleDefinition;


        [TestInitialize]
        public void Initialize()
        {
            m_moduleDefinition = ModuleDefinition.ReadModule(typeof(string).Assembly.Location);
        }

        [TestCleanup]
        public void Cleanup()
        {
            m_moduleDefinition.Dispose();
        }

        [TestMethod]
        public void GetMethodFluent()
        {
            m_moduleDefinition.ImportFluent<Stopwatch>()
                .GetMethod(s => s.Start, out MethodDefinition startsWithDef);

            Assert.IsNotNull(startsWithDef);
        }

        [TestMethod]
        public void GetMethod()
        {
            m_moduleDefinition.ImportFluent<Stopwatch>()
                .GetMethod(nameof(Stopwatch.Start), out MethodDefinition startsWithDef);

            Assert.IsNotNull(startsWithDef);
        }

        [TestMethod]
        public void GetStaticMethod()
        {
            m_moduleDefinition.ImportFluent<string>()
                .GetMethod(() => string.Concat(string.Empty, string.Empty), out MethodDefinition concat);

            Assert.IsNotNull(concat);
            Assert.AreEqual(2, concat.Parameters.Count);
        }


        [TestMethod]
        public void GetMethodWithNull()
        {
            // By Name
            Assert.ThrowsException<System.ArgumentNullException>(() =>
            m_moduleDefinition.ImportFluent<Stopwatch>()
                .GetMethod(methodName: null, out MethodDefinition _));

            // Expression 
            Assert.ThrowsException<System.ArgumentNullException>(() =>
            m_moduleDefinition.ImportFluent<Stopwatch>()
                .GetMethod(expression:null, out MethodDefinition _));
        }
    }
}

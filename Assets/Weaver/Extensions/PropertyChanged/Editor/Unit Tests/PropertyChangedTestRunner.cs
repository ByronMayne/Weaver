using NUnit.Framework;
using System.CodeDom.Compiler;
using System.Reflection;
using Fasterflect;
using System;
using System.IO;

namespace Weaver.Tests
{
    public class TestRunner : BaseWeaverTest
    {
        private Assembly m_CompiledAssembly;
        private string m_TestPath;

        [SetUp]
        public override void Setup()
        {
            m_TestPath = "./Assets/Weaver/Extensions/PropertyChanged/Editor/Unit Tests/Test_PropertyChanged_Calbacks.txt";
            base.Setup();
            // Compile our test
            AssemblyTestResult result = CompileTest(SourceProvider);
            // Get the assembly
            m_CompiledAssembly = result.weavedAssembly;
        }

        [Test]
        [Description("Verifies that the OnChanged callback happens when we have a valid target.")]
        public void TestCallbacks()
        {
            // Get our test type
            Type callbackTestType = m_CompiledAssembly.GetType("Test_PropertyChanged_Calbacks");
            // We are assuming we are not null for this test
            Assume.That(callbackTestType != null, "Unable to get the type Test_PropertyChanged_Calbacks from test assembly");
            // Create an instance of it
            object instance = Activator.CreateInstance(callbackTestType);
            // Make sure it's not null
            Assume.That(instance != null, "Unable to create instance of 'Test_PropertyChanged_Calbacks'");
            // Make sure our attributes were removed
            int startingAge = (int)instance.GetPropertyValue("age");
            int newAge = startingAge + 103;
            bool callbackInvoked = false;
            Action<int> onAgeChanged = (Action<int>)instance.GetFieldValue("onAgeChanged");
            // Subscribe our callback
            onAgeChanged += (int setAge) =>
            {
                // Make sure the age being set is correct
                Assert.AreEqual(setAge, newAge, "The age argument being set in not correct");
                // Set that our callback was set
                callbackInvoked = true;
            };
            // Invoke our setter
            instance.SetPropertyValue("age", newAge);
            // Make sure our age is equal
            Assert.AreEqual(instance.GetPropertyValue("age"), newAge, "For some reason age was not set to the correct value");
            // If our callback was invoked our test has passed
            Assert.True(callbackInvoked, "The callback was not invoked for setting the age.");
        }

        private string SourceProvider()
        {
            string source = File.ReadAllText(m_TestPath);
            return source;
        }
    }
}

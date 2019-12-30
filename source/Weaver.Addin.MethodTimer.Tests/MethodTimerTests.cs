using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weaver.Contracts;
using Weaver.Addin.MethodTimer.Editor;
using Weaver.Tests;
using System.Collections.Generic;
using System;

namespace Weaver.Addin.MethodTimer.Tests
{
    [TestClass]
    public class MethodTimerTests : BaseComplicationTest
    {
        [TestMethod]
        public void BasicCompile()
        {
            IAssemblyWeaver weaver = new AssemblyWeaver();

            string source =
            @"using Weaver.Addin.MethodTimer;

            public class BasicExample
            {
                [MethodTimer]
                public static void Start()
                {

                }
            }";

            string path = Compile(source);
   
            weaver.WeaveAssembly(path, new[] { new MethodTimerAddin() });
        }

        [TestMethod]
        public void MultiReturn()
        {
            IAssemblyWeaver weaver = new AssemblyWeaver();

            string source =
            @"using Weaver.Addin.MethodTimer;

            public class BasicExample
            {
                [MethodTimer]
                public static int Start(int input)
                {
                    int count = 4;
                    if(input < 0)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }

                    return 4;
                }
            }";

            string path = Compile(source);

            weaver.WeaveAssembly(path, new[] { new MethodTimerAddin() });
        }

        protected override void AppendReferences(ISet<string> assemblyPaths)
        {
            assemblyPaths.Add(typeof(MethodTimerAttribute).Assembly.Location);
            assemblyPaths.Add(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.3\build\netstandard2.0\ref\netstandard.dll");
        }
    }
}

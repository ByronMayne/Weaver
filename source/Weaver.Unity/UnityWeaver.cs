using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Seed.IO;
using UnityEngine;
using Weaver.Contracts;
using ILogger = Weaver.Contracts.Diagnostics.ILogger;

namespace Weaver.Unity
{
    public class UnityWeaver
    {
        private readonly IAssemblyWeaver m_weaver;
        private readonly ILogger m_logger;
        private readonly ICollection<IWeaverAddin> m_addins;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWeaver"/> class. Created in
        /// the <see cref="EntryPoint"/> class. 
        /// </summary>
        public UnityWeaver()
        {
            // Create instances 
            m_addins = AppDomain.CurrentDomain
                .GetAssemblies()
                // These assemblies can't have add-ins 
                .Where(a => !a.FullName.StartsWith("Microsoft"))
                .Where(a => !a.FullName.StartsWith("Unity"))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => typeof(IWeaverAddin).IsAssignableFrom(t))
                .Select(CreateInstance)
                .Where(i => i != null)
                .ToArray(); // We don't want them to load lazy 

            m_logger = new UnityLog();

            m_weaver = new AssemblyWeaver();
            m_weaver.Logger = m_logger;
            m_weaver.WorkingDirectory = Application.dataPath;
        }

        /// <summary>
        /// Creates the instance of an type as long as it has a constructor that has no parameters. 
        /// </summary>
        private IWeaverAddin CreateInstance(Type type)
        {
            ConstructorInfo constructor = type.GetConstructors()
                .Where(c => c.GetParameters().Length == 0)
                .FirstOrDefault(); 

            if(constructor == null)
            {
                Debug.LogError($"Unable to create instance of {type.FullName} as it does not contain a constructor that takes no arguments.");
                return null;
            }
            return (IWeaverAddin)constructor.Invoke(Array.Empty<object>());
        }

        internal void WeaveAssembly(AbsolutePath assemblyPath)
        {
            Debug.Log($"WeaveAssembly: {assemblyPath}");

            m_weaver.WeaveAssembly(assemblyPath, m_addins);
        }
    }
}

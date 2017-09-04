using Mono.Cecil;
using System;
using UnityEditorInternal;

namespace Weaver
{
    /// <summary>
    /// Used to resolve any references to the Unity Editor or UnityEngine assemblies.
    /// </summary>
    public class WeaverAssemblyResolver : DefaultAssemblyResolver
    {
        public override AssemblyDefinition Resolve(string fullName)
        {
            if(IsUnityEngineAssembly(fullName))
            {
                return GetUnityEngineAssemblyDef();
            }

            if(IsUnityEditorAssembly(fullName))
            {
                return GetUnityEditorAssemblyDef(); 
            }

            return base.Resolve(fullName);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (IsUnityEngineAssembly(name.FullName))
            {
                return GetUnityEngineAssemblyDef();
            }

            if (IsUnityEditorAssembly(name.FullName))
            {
                return GetUnityEditorAssemblyDef();
            }

            return base.Resolve(name);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (IsUnityEngineAssembly(name.FullName))
            {
                return GetUnityEngineAssemblyDef();
            }

            if (IsUnityEditorAssembly(name.FullName))
            {
                return GetUnityEditorAssemblyDef();
            }

            return base.Resolve(name, parameters);
        }

        public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            if (IsUnityEngineAssembly(fullName))
            {
                return GetUnityEngineAssemblyDef();
            }

            if (IsUnityEditorAssembly(fullName))
            {
                return GetUnityEditorAssemblyDef();
            }

            return base.Resolve(fullName, parameters);
        }

        private static bool IsUnityEditorAssembly(string name)
        {
            return name.Equals("UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", StringComparison.Ordinal);
        }

        private static bool IsUnityEngineAssembly(string name)
        {
            return name.Equals("UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", StringComparison.Ordinal);
        }

        private static AssemblyDefinition GetUnityEngineAssemblyDef()
        {
            string engineAssemblyPath = InternalEditorUtility.GetEngineAssemblyPath();
            return AssemblyDefinition.ReadAssembly(engineAssemblyPath);
        }

        private static AssemblyDefinition GetUnityEditorAssemblyDef()
        {
            string editorAssemblyPath = InternalEditorUtility.GetEditorAssemblyPath();
            return AssemblyDefinition.ReadAssembly(editorAssemblyPath);
        }
    }
}

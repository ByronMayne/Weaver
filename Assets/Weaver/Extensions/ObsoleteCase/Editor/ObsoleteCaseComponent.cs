using System;
using Mono.Cecil;
using UnityEngine;
using Weaver.Extensions;

namespace Weaver
{
    public class ObsoleteCaseComponent : WeaverComponent
    {
        private Version m_UnityVersion; 
        private TypeImplementation m_ObsoleteAttributeTypeImp;
        private MethodImplementation m_ObsoleteAttributeConstructorMethodImp;

        public override string addinName
        {
            get
            {
                return "Obsolete Case";
            }
        }

        public override DefinitionType effectedDefintions
        {
            get
            {
                return DefinitionType.All;
            }
        }

        public override void VisitModule(ModuleDefinition moduleDefinition)
        {
            m_UnityVersion = GetValidVersion(Application.unityVersion);
            m_ObsoleteAttributeTypeImp = new TypeImplementation(moduleDefinition, typeof(ObsoleteAttribute));
            m_ObsoleteAttributeConstructorMethodImp = m_ObsoleteAttributeTypeImp.GetConstructor(typeof(string), typeof(bool));
        }

        private Version GetValidVersion(string version)
        {
            char[] versionCode = new char[version.Length];
            for(int i=  0; i < versionCode.Length; i++)
            {
                char code = version[i];
                switch(code)
                {
                    case 'f': code = '3'; break; // Final
                    case 'p': code = '2'; break; // Patch
                    case 'b': code = '1'; break; // Beta
                }
                versionCode[i] = code;
            }
            return new Version(new string(versionCode));
        }

        public override void VisitType(TypeDefinition typeDefinition)
        {
            // Get our attribute
            CustomAttribute obsoleteCaseAttribute = typeDefinition.GetCustomAttribute<ObsoleteCaseAttribute>();
            // Check if it's null
            if (obsoleteCaseAttribute == null)
            {
                // We don't have one so we quit.
                return;
            }
            // Remove our old attribute
            typeDefinition.CustomAttributes.Remove(obsoleteCaseAttribute);
            // Get values
            string messageString = obsoleteCaseAttribute.GetValue<string>("message");
            string errorVersionString = obsoleteCaseAttribute.GetValue<string>("treatAsErrorFromVersion");
            string removedInVersionString = obsoleteCaseAttribute.GetValue<string>("removedInVersion");
            Version errorVersion = GetValidVersion(errorVersionString);
            Version removedInVersion = GetValidVersion(removedInVersionString);
            string replacementTypeOrMemberString = obsoleteCaseAttribute.GetValue<string>("replacementTypeOrMember");

            string constructorMessage;

            if( errorVersion < m_UnityVersion)
            {
                constructorMessage = string.Format("{0}. Use '{1}' instead. This was deperacted in version {2} and has been removed since versoin {3}. Current Unity Version {4}.",
                    messageString, 
                    replacementTypeOrMemberString,
                    removedInVersionString, 
                    errorVersionString,
                    Application.unityVersion);
            }
            else if ( removedInVersion < m_UnityVersion)
            {
                constructorMessage = string.Format("{0}. Use '{1}' instead. This was deperacted in version {2} and Will be removed in version {3}. Current Unity Version {4}.",
                    messageString,
                    replacementTypeOrMemberString,
                    removedInVersionString,
                    errorVersionString,
                    Application.unityVersion);
            }
            else
            {
                // This attribute is not needed yet.
                return;
            }

            CustomAttribute obsoleteAttribute = new CustomAttribute(m_ObsoleteAttributeConstructorMethodImp.reference);
            CustomAttributeArgument messageArgument = new CustomAttributeArgument(typeSystem.String, constructorMessage);
            obsoleteAttribute.ConstructorArguments.Add(messageArgument);

            bool isError = m_UnityVersion > errorVersion;
            CustomAttributeArgument isErrorArgument = new CustomAttributeArgument(typeSystem.Boolean, isError);
            obsoleteAttribute.ConstructorArguments.Add(isErrorArgument);
            typeDefinition.CustomAttributes.Add(obsoleteAttribute);
        }
    }
}

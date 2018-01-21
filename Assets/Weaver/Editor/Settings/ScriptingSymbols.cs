using System;
using UnityEditor;
using UnityEngine;

namespace Weaver
{
    [Serializable]
    public struct ScriptingSymbols
    {
        [SerializeField]
        public string value;
        [SerializeField]
        private bool m_IsActive;

        /// <summary>
        /// Returns back true if the symbols are defined. 
        /// </summary>
        public bool isActive
        {
            get { return m_IsActive; }
        }

        public void ValidateSymbols()
        {
            if (string.IsNullOrEmpty(value))
            {
                m_IsActive = true;
                return;
            }

            char[] spitKey = new char[] { ';' };
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            string[] requiredDefines = value.Split(spitKey, StringSplitOptions.RemoveEmptyEntries);
            string[] activeDefines = EditorUserBuildSettings.activeScriptCompilationDefines;

            foreach (string user in requiredDefines)
            {
                bool wasFound = false;
                bool isInversed = user[0] == '!';
                int indexA = isInversed ? 1 : 0;

                foreach (string current in activeDefines)
                {

                    // Make sure we are the same length 
                    if (user.Length - indexA != current.Length)
                    {
                        continue;
                    }

                    if (string.Compare(user, indexA, current, 0, current.Length) == 0)
                    {
                        wasFound = true;
                        break;
                    }
                }

                if (wasFound == isInversed)
                {
                    m_IsActive = false;
                    return;
                }
            }

            m_IsActive = true;
        }
    }
}
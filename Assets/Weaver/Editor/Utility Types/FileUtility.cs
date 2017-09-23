using UnityEditor;
using UnityEngine;

namespace Weaver
{
    [InitializeOnLoad]
    public static class FileUtility
    {
        private readonly static string m_ProjectPath;
        private readonly static int m_ProjectPathLength; 

        static FileUtility()
        {
            // Get our data path
            m_ProjectPath = Application.dataPath;
            // Remove 'Assets'
            m_ProjectPath = m_ProjectPath.Substring(0, m_ProjectPath.Length - /* Assets */ 6);
            // Store our lenth
            m_ProjectPathLength = m_ProjectPath.Length;
        }

        /// <summary>
        /// Gets the folder at the root of the project below 'Assets'
        /// </summary>
        public static string projectPath
        {
            get { return m_ProjectPath; }
        }

        /// <summary>
        /// Converts a full System Path to a Unity project relative path.
        /// </summary>
        public static string SystemToProjectPath(string systemPath)
        {
            // Get our path
            string path = projectPath;
            // Get the length
            int dataPathLength = path.Length;
            // Split it
            path = systemPath.Substring(dataPathLength, m_ProjectPathLength - dataPathLength);
            // Return the result
            return path;
        }
    }
}
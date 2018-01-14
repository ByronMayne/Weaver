using UnityEditor;
using UnityEngine;

namespace Weaver.Analytics
{
    public class AnalyticState
    {
        private static AnalyticState _instance;

        [SerializeField]
        private bool _isFirstLaunch = true;
        [SerializeField]
        private double _timeSinceStartup = 0D;

        /// <summary>
        /// Gets or sets if this is the first launch of Weaver on this computer 
        /// </summary>
        public static bool IsFirstLaunchOnMachine
        {
            get { return instance._isFirstLaunch; }
            set
            {
                if (instance._isFirstLaunch != value)
                {
                    instance._isFirstLaunch = value;
                    SaveState();
                }
            }
        }


        public static bool IsNewSession()
        {
            if (EditorApplication.timeSinceStartup < instance._timeSinceStartup)
            {
                instance._timeSinceStartup = EditorApplication.timeSinceStartup;
                SaveState();
                return true; 
            }
            return false; 
        }

        /// <summary>
        /// Returns back an instance of Analytic state.
        /// </summary>
        private static AnalyticState instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadState();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the play pref save key that we use to track this version 
        /// </summary>
        private static string saveKey
        {
            get { return string.Format("{0}.{1}", AnalyticsConstants.STATE_EDITOR_PREFS_KEY, Application.productName); }
        }

        /// <summary>
        /// Creates a new state if one does not exist and populates it 
        /// with any information that we stored in editor prefs. 
        /// </summary>
        public static void LoadState()
        {
            if (_instance == null)
            {
                _instance = new AnalyticState();
            }

            if (EditorPrefs.HasKey(AnalyticsConstants.STATE_EDITOR_PREFS_KEY))
            {
                string json = EditorPrefs.GetString(AnalyticsConstants.STATE_EDITOR_PREFS_KEY);

                if (!string.IsNullOrEmpty(json))
                {
                    JsonUtility.FromJsonOverwrite(json, _instance);
                }
            }
        }

        /// <summary>
        /// Saves the current state to disk. 
        /// </summary>
        public static void SaveState()
        {
            if (_instance != null)
            {
                string json = JsonUtility.ToJson(_instance);
                EditorPrefs.SetString(AnalyticsConstants.STATE_EDITOR_PREFS_KEY, json);
            }
        }
    }
}
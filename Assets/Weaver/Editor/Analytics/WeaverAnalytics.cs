using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditorInternal;
using System.Text;
using System.Security.Cryptography;

namespace Weaver.Analytics
{
    public enum HitType
    {
        Event,
        PageView,
        ScreenView,
        Transaction,
        Item,
        Social,
        Exception,
        Timing,
    }

    /// <summary>
    /// Weaver Analytics is used to help make this tool better. I report any errors or exceptions
    /// that you receive while using the tool. I do not send any information about the content of
    /// your project besides some basic information about the components you have made. If you don't
    /// want to have anything sent just remove the pre-processor directive above. 
    /// </summary>
    public static class WeaverAnalytics
    {
        public static void OnSettingsEnabled(WeaverSettings settings)
        {
            if (AnalyticState.IsFirstLaunchOnMachine)
            {
                SendEvent("User", "FirstLaunch", "", null);
            }
            if (AnalyticState.IsNewSession())
            {
                SendEvent("User", "HasPro", InternalEditorUtility.HasPro().ToString(), null);
                SendEvent("User", "ScriptCount", AssetDatabase.FindAssets("t:Script").Length.ToString(), null);
            }
        }

        public static void SendException(string exceptionDescription, bool isFatal)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData["exd"] = exceptionDescription;
            postData["exf"] = isFatal.ToString();
            Send(postData, HitType.Exception);
        }

        public static void SendTiming(string variableName, long length)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData["utv"] = variableName;
            postData["utt"] = length.ToString();
            Send(postData, HitType.Timing);
        }

        public static void SendEvent(string category, string action, string label, int? value = null)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            if (value.HasValue)
            {
                postData.Add("ev", value.ToString());
                postData["ec"] = category;
                postData["ea"] = action;
                if (!string.IsNullOrEmpty(label))
                {
                    postData["el"] = label;
                }
            }
            Send(postData, HitType.Event);
        }

        /// <summary>
        /// Sends a new analytic event to the server. 
        /// </summary>
        /// <param name="hitType">The type of event we have</param>
        /// <param name="category">The category</param>
        /// <param name="action">The action key</param>
        /// <param name="label">the label on the action</param>
        /// <param name="value">The value of the action</param>
        public static void Send(Dictionary<string, string> postData, HitType hitType)
        {
            postData["v"] = AnalyticsConstants.PROTOCOL_VERSION;
            postData["ds"] = InternalEditorUtility.GetFullUnityVersion() + ":" + Application.platform.ToString();
            postData["tid"] = AnalyticsConstants.TRACKING_ID;
            postData["uid"] = GetUserID();
            postData["cid"] = GetClientID();
            postData["t"] = hitType.ToString().ToLower();
            postData["av"] = WeaverSettings.VERSION;

            // Create our request
            UnityWebRequest www = UnityWebRequest.Post(AnalyticsConstants.URL, postData);
            // Send the even t
            www.SendWebRequest();
        }

        /// <summary>
        /// People most likely don't want their product name being sent
        /// with Analytics so we encode it. I just need to to be unique :D 
        /// </summary>
        private static string GetClientID()
        {
            return EncodeString(Application.companyName + ":" + Application.productName);
        }

        private static string GetUserID()
        {
            return EncodeString(System.Environment.UserName);
        }

        private static string EncodeString(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}

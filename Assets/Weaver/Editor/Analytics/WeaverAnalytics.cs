using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditorInternal;
using System;
using System.Globalization;

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

        public static void SendTiming(string catagory, string variableName, long length)
        {
            SendTiming(catagory, variableName, length, null);
        }

        public static void SendTiming(string catagory, string variableName, long length, string label)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData["utc"] = catagory; //User timing category
            postData["utv"] = variableName; // User timing variable name
            postData["utt"] = length.ToString(); //User timing time
            if (!string.IsNullOrEmpty(label))
            {
                postData["utl"] = label; // Event Label
            }
            Send(postData, HitType.Timing);
        }


        public static void SendEvent(string category, string action, string label, int? value = null)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData["ec"] = category; // Event Category
            postData["ea"] = action; // Event Action
            if (value.HasValue)
            {
                postData.Add("ev", value.ToString()); // Event Value
            }
            if (!string.IsNullOrEmpty(label))
            {
                postData["el"] = label; // Event Label
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
            postData["t"] = hitType.ToString().ToLower(); // Hit Type
            postData["v"] = AnalyticsConstants.PROTOCOL_VERSION; // Protocol Version
            postData["tid"] = AnalyticsConstants.TRACKING_ID; // Tracking ID 
            postData["ds"] = InternalEditorUtility.GetFullUnityVersion(); // DataSource
            postData["cid"] = GetClientID(); // Client ID
            postData["uid"] = GetUserID(); // User ID
            postData["av"] = WeaverSettings.VERSION; // Application Version

            // Create our request
            UnityWebRequest www = UnityWebRequest.Post(AnalyticsConstants.URL, postData);
            // Send the even t
            UnityWebRequestAsyncOperation asyncOp = www.SendWebRequest();
            // Subscribe
            asyncOp.completed += OnRequestComplete;
        }

        private static void OnRequestComplete(AsyncOperation asyncOp)
        {
            Debug.Log("Request Sent: " + asyncOp.isDone);
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
            return EncodeString(Environment.MachineName);
        }

        private static string EncodeString(string input)
        {
            uint result;
            unchecked
            {
                uint hash1 = 5381;
                uint hash2 = hash1;

                for (int i = 0; i < input.Length && input[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ input[i];
                    if (i == input.Length - 1 || input[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ input[i + 1];
                }

                result = hash1 + (hash2 * 1566083941);
            }
            return result.ToString();
        }
    }
}

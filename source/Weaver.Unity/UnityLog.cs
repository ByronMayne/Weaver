using System;

namespace Weaver.Unity
{
    public class UnityLog : Contracts.Diagnostics.ILogger
    {
        public void Debug(string channel, string message)
        {
            UnityEngine.Debug.Log($"[{channel}] {message}");
        }

        public void Error(string channel, string message)
        {
            UnityEngine.Debug.LogError($"[{channel}] {message}");
        }

        public void Exception(string channel, Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        public void Info(string channel, string message)
        {
            UnityEngine.Debug.Log($"[{channel}] {message}");
        }

        public void Warning(string channel, string message)
        {
            UnityEngine.Debug.LogWarning($"[{channel}] {message}");
        }
    }
}

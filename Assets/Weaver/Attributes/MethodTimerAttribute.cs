using System;

namespace Weaver
{
    /// <summary>
    /// Based off of https://github.com/Fody/MethodTimer
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MethodTimerAttribute : Attribute
    {
    }
}

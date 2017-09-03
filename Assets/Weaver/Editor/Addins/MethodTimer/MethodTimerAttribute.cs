using System;

namespace Weaver
{
    /// <summary>
    /// Based off of https://github.com/Fody/PropertyChanged
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MethodTimerAttribute : Attribute
    {
    }
}

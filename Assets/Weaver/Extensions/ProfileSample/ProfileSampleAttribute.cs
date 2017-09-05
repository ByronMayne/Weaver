using System;

namespace Weaver
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProfileSampleAttribute : Attribute
    {
    }
}

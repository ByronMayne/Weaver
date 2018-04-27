using System;

namespace Weaver
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OnChangedAttribute : Attribute
    {
        public string callbackMethod { get; set; }

        public bool isValidated { get; set; }

        public OnChangedAttribute(string callbackMethod)
        {
            this.callbackMethod = callbackMethod;
        }
    }
}
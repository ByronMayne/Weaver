using System;

namespace Weaver.Addin.MethodTimer
{
    public sealed class MethodTimerAttribute : Attribute
    {
        public bool Recursive { get; }

        public MethodTimerAttribute()
        {
            Recursive = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodTimerAttribute"/> class.
        /// </summary>
        /// <param name="isRecursive">if set to <c>true</c> this function and every 
        /// function it calls into will have the method logged otherwise just this function will. As note this will make the weaving time slower</param>
        public MethodTimerAttribute(bool isRecursive)
        {
            Recursive = isRecursive;
        }
    }
}

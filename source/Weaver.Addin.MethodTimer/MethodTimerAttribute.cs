using System;

namespace Weaver.Addin.MethodTimer
{
    public class MethodTimerAttribute : Attribute
    {
        /// <summary>
        /// The delegate for invoking logs 
        /// </summary>
        /// <param name="declaringClass">The declaring class.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="duration">The duration.</param>
        public delegate void MethodTimerDelegate(string declaringClass, string methodName, TimeSpan duration);

        /// <summary>
        /// Invoked whenever any method has been logged.
        /// </summary>
        public static MethodTimerDelegate OnMethodLogged;
    }
}

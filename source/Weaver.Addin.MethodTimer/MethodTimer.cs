using System;

namespace Weaver.Addin.MethodTimer
{
    public static class MethodTimer
    {
        /// <summary>
        /// The delegate for invoking logs 
        /// </summary>
        /// <param name="declaringType">The declaring class type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="duration">The duration.</param>
        public delegate void MethodTimerDelegate(Type declaringType, string methodName, TimeSpan duration);

        /// <summary>
        /// Invoked whenever any method has been logged.
        /// </summary>
        public static MethodTimerDelegate OnMethodLogged;
    }
}

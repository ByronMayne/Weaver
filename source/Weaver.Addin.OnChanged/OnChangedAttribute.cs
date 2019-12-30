using System;
using System.ComponentModel;

namespace Weaver.Addin.OnChanged
{
    /// <summary>
    /// When applied to a property this will invoke a selected method 'BEFORE' the value is applied. 
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class OnChangedAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the method we invoke when one of our properties changes 
        /// </summary>
        public string CallbackMethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnChangedAttribute"/> class. Since no callback
        /// method is defined we expect the interface <see cref="INotifyPropertyChanged"/> otherwise this will be an error
        /// </summary>
        public OnChangedAttribute()
        {
            CallbackMethodName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnChangedAttribute"/> class. The method name must exist in the current class.
        /// </summary>
        /// <param name="callbackMethodName">Name of the callback method.</param>
        public OnChangedAttribute(string callbackMethodName)
        {
            CallbackMethodName = callbackMethodName;
        }
    }
}

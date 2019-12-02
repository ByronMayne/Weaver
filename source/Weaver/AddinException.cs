using System;
using Weaver.Contracts;

namespace Weaver
{
    /// <summary>
    /// A wrapper around an exception that is thrown from a Addin while weaving. 
    /// </summary>
    public class AddinException : Exception
    {
        public IWeaverAddin Context { get; }

        public AddinException(IWeaverAddin context, Exception innerException) : base(FormatError(context), innerException)
        {
            Context = context;
        }

        private static string FormatError(IWeaverAddin weaverAddin)
        {
            return $"An exception was thrown by the addin {weaverAddin.Name} [{weaverAddin.GetType().FullName} durning weaving process. See inner exception for more details";
        }
    }
}

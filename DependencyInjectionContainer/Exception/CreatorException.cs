using System;
using System.Runtime.Serialization;

namespace DependencyInjectionContainer.Exception
{
    [Serializable]
    public class CreatorException : System.Exception
    {
        public CreatorException()
        {
        }

        protected CreatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CreatorException(string message) : base(message)
        {
        }

        public CreatorException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}

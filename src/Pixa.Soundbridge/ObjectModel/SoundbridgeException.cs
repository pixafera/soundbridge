using System;
using System.Runtime.Serialization;

namespace Pixa.Soundbridge.Library
{
    [Serializable()]
    public class SoundbridgeException : Exception
    {
        public SoundbridgeException()
        {
        }

        public SoundbridgeException(string message) : base(message)
        {
        }

        public SoundbridgeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SoundbridgeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
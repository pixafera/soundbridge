using System;
using System.Runtime.Serialization;

namespace Pixa.Soundbridge.Client
{
    [Serializable()]
    public class SoundbridgeClientException : Exception
    {
        public SoundbridgeClientException()
        {
        }

        public SoundbridgeClientException(string message) : base(message)
        {
        }

        public SoundbridgeClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SoundbridgeClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
using System;
using System.Runtime.Serialization;

namespace Pixa.Soundbridge.Client
{
    public class SoundbridgeCommandException : SoundbridgeClientException
    {
        private string _command;

        public SoundbridgeCommandException(string command) : base()
        {
            _command = command;
        }

        public SoundbridgeCommandException(string command, string message) : base(message)
        {
            _command = command;
        }

        public SoundbridgeCommandException(string command, string message, Exception innerException) : base(message, innerException)
        {
            _command = command;
        }

        protected SoundbridgeCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
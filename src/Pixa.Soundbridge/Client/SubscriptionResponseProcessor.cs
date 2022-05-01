using System;
using System.Threading;

namespace Pixa.Soundbridge.Client
{

    /// <summary>
    /// Processes responses for subscription RCP methods.
    /// </summary>
    /// <remarks></remarks>
    internal class SubscriptionResponseProcessor : ResponseProcessorBase
    {
        private Action<string> _eventRaiser;
        private bool _receivedSubAck;

        /// <summary>
        /// Instantiates a new instance of ResponseProcessorBase for the specified SoundbridgeClient and EventWaitHandle.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="waitHandle"></param>
        /// <remarks></remarks>
        public SubscriptionResponseProcessor(TcpSoundbridgeClient client, string command, EventWaitHandle waitHandle, Action<string> eventRaiser) : base(client, command, waitHandle)
        {
            _eventRaiser = eventRaiser;
        }

        /// <summary>
        /// Processes the specified response line.
        /// </summary>
        /// <param name="response"></param>
        /// <remarks></remarks>
        public override void Process(string response)
        {
            if (_receivedSubAck)
            {
                _eventRaiser(response);
            }
            else
            {
                AddResponse(response);
                _receivedSubAck = true;
                WaitHandle.Set();
            }
        }

        public override void PostProcess()
        {
            if (Response.Length == 0)
                ExceptionHelper.ThrowCommandTimeout(Command);
        }
    }
}
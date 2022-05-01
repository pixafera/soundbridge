using System;
using System.Threading;

namespace Pixa.Soundbridge.Client {

    /// <summary>
    /// Contains metadata about Subscription RCP Commands.
    /// </summary>
    /// <remarks></remarks>
    public sealed class RcpSubscriptionCommandAttribute : RcpCommandAttribute {
        private string _eventRaiserMethodName;

        /// <summary>
        /// Initialises a new instance of <see cref="RcpSubscriptionCommandAttribute"/>.
        /// </summary>
        /// <param name="command">The name of the command to be sent to the
        /// soundbridge.</param>
        /// <param name="eventRaiserMethodName">The name of the method to be called
        /// when a subscription notification is received.</param>
        public RcpSubscriptionCommandAttribute(string command, string eventRaiserMethodName) : base(command) {
            _eventRaiserMethodName = eventRaiserMethodName;
        }

        /// <summary>
        /// Gets the name of the method to be called when a subscription notification
        /// is reeived.
        /// </summary>
        public string EventRaiserMethodName {
            get {
                return _eventRaiserMethodName;
            }
        }

        /// <summary>
        /// Creates a <see cref="IResponseProcessor"/> to handle responses from this
        /// command.
        /// </summary>
        /// <param name="client">The <see cref="TcpSoundbridgeClient"/> to handle
        /// responses for.</param>
        /// <param name="waitHandle">The <see cref="EventWaitHandle"/> to signal when
        /// a response is received.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override IResponseProcessor CreateResponseProcessor(TcpSoundbridgeClient client, EventWaitHandle waitHandle) {
            Action<string> d;
            try {
                d = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), client, EventRaiserMethodName);
            } catch (ArgumentException aex) {
                throw new MissingMethodException(string.Format("The method {0} could not be found on {1}", EventRaiserMethodName, client.GetType().FullName));
            }

            return new SubscriptionResponseProcessor(client, Command, waitHandle, d);
        }
    }
}
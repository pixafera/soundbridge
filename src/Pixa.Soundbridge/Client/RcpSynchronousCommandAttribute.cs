using System;

namespace Pixa.Soundbridge.Client {
    /// <summary>
    /// Provides metadata about synchronous RCP commands.
    /// </summary>
    /// <remarks></remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RcpSynchronousCommandAttribute : RcpCommandAttribute {
        private bool _isList;

        public RcpSynchronousCommandAttribute(string command) : this(command, false) {
        }

        public RcpSynchronousCommandAttribute(string command, bool isList) : base(command) {
            _isList = isList;
        }

        public bool IsList {
            get {
                return _isList;
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="IResponseProcessor"/> capable of deal with the responses of an RCP server to synchronous commands.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="waitHandle"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override IResponseProcessor CreateResponseProcessor(TcpSoundbridgeClient client, System.Threading.EventWaitHandle waitHandle) {
            return new SynchronousResponseProcessor(client, Command, waitHandle, _isList);
        }
    }
}
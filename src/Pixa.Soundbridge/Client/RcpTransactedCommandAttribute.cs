using System;
using System.Threading;

namespace Pixa.Soundbridge.Client {

    /// <summary>
    /// Contains metadata about Transacted RCP commands.
    /// </summary>
    /// <remarks></remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RcpTransactedCommandAttribute : RcpCommandAttribute {
        public RcpTransactedCommandAttribute(string command) : base(command) {
        }

        // TODO: Implement IsList
        public RcpTransactedCommandAttribute(string command, bool isList) : this(command) {
        }

        public override IResponseProcessor CreateResponseProcessor(TcpSoundbridgeClient client, EventWaitHandle waitHandle) {
            return new TransactedResponseProcessor(client, Command, waitHandle);
        }
    }
}
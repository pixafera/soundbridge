using System;
using System.Threading;

namespace Pixa.Soundbridge.Client
{

    /// <summary>
    /// Acts as a base class for providing metadata about an RCP command.
    /// </summary>
    /// <remarks></remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class RcpCommandAttribute : Attribute
    {
        private string _command;

        public RcpCommandAttribute(string command)
        {
            _command = command;
        }

        public string Command
        {
            get
            {
                return _command;
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="IResponseProcessor"/> that can deal with the responses an RCP server will give to this command.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="waitHandle"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract IResponseProcessor CreateResponseProcessor(TcpSoundbridgeClient client, EventWaitHandle waitHandle);
    }
}
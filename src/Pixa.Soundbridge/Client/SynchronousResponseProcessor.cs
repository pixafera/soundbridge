using System.Diagnostics;
using System.Threading;

namespace Pixa.Soundbridge.Client
{

    /// <summary>
    /// Processes responses to synchronous RCP methods.
    /// </summary>
    /// <remarks></remarks>
    internal class SynchronousResponseProcessor : ResponseProcessorBase
    {
        private int _responseLength = 1;
        private bool _isList;

        /// <summary>
        /// Instantiates a new instance of SynchronousResponseProcessor for the specified SoundbridgeClient and EventWaitHandle.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="waitHandle"></param>
        /// <remarks></remarks>
        public SynchronousResponseProcessor(TcpSoundbridgeClient client, string command, EventWaitHandle waitHandle) : this(client, command, waitHandle, false)
        {
        }

        /// <summary>
        /// Instantiates a new instance of SynchronousResponseProcessor for the specified SoundbridgeClient and command, indicating whether or not the results will be a list and if so, what error values to look out for.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="command"></param>
        /// <param name="waithandle"></param>
        /// <param name="isList"></param>
        /// <remarks></remarks>
        public SynchronousResponseProcessor(TcpSoundbridgeClient client, string command, EventWaitHandle waithandle, bool isList) : base(client, command, waithandle)
        {
            _isList = isList;
        }

        /// <summary>
        /// Gets whether the command will return a list or not
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsList
        {
            get
            {
                return _isList;
            }
        }

        /// <summary>
        /// Processes the specified response line.
        /// </summary>
        /// <param name="response"></param>
        /// <remarks></remarks>
        public override void Process(string response)
        {
            if (response.StartsWith("ListResultSize"))
            {
                int.TryParse(response.Substring(15), out _responseLength);
                return;
            }

            if (response == "ListResultEnd")
                return;
            AddResponse(response);
            if (ResponseCount == _responseLength)
                WaitHandle.Set();
        }

        public override void PostProcess()
        {
            if (Response.Length == 0)
                ExceptionHelper.ThrowCommandTimeout(Command);

            /* TODO ERROR: Skipped IfDirectiveTrivia
            #If DEBUG Then
            */
            if (Response.Length == 1 & Response[0] == "TransactionInitiated")
            {
                Debug.WriteLine(string.Format("The command '{0}' appears to be a transacted command, but was handled by a SynchronousResponseProcessor", Command));
            }
            /* TODO ERROR: Skipped EndIfDirectiveTrivia
            #End If
            */
        }
    }
}
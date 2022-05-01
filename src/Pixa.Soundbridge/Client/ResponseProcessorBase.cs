using System.Collections.Generic;
using System.Threading;

namespace Pixa.Soundbridge.Client {

    /// <summary>
    /// Provides base functionality for processing responses from the Soundbridge and signalling the requesting thread.
    /// </summary>
    /// <remarks></remarks>
    public abstract class ResponseProcessorBase : IResponseProcessor {

        /// <summary>
        /// Instantiates a new instance of ResponseProcessorBase for the specified SoundbridgeClient and EventWaitHandle.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="waitHandle"></param>
        /// <remarks></remarks>
        public ResponseProcessorBase(ISoundbridgeClient client, string command, EventWaitHandle waitHandle) {
            _client = (TcpSoundbridgeClient)client;
            _command = command;
            _waitHandle = waitHandle;
        }

        #region  Client 
        private TcpSoundbridgeClient _client;

        /// <summary>
        /// Gets the client this ResponseProcessorBase is associated with.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TcpSoundbridgeClient Client {
            get {
                return _client;
            }
        }
        #endregion

        #region  Command 
        private string _command;

        public string Command {
            get {
                return _command;
            }
        }
        #endregion

        #region  WaitHandle 
        private EventWaitHandle _waitHandle;

        /// <summary>
        /// Gets the EventWaitHandle this ResponseProcessorBase will signal when the entire response has been received.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        protected EventWaitHandle WaitHandle {
            get {
                return _waitHandle;
            }
        }
        #endregion

        #region  Response 
        private List<string> _response = new List<string>();
        private bool _byteResponse;

        public bool IsByteArray {
            get {
                return _byteResponse;
            }

            set {
                _byteResponse = value;
            }
        }

        /// <summary>
        /// Gets the number of lines in the response.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int ResponseCount {
            get {
                return _response.Count;
            }
        }

        /// <summary>
        /// Gets the lines in the response.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string[] Response {
            get {
                return _response.ToArray();
            }
        }

        /// <summary>
        /// Adds a line to the list of response lines that are of interest to consuming clients.
        /// </summary>
        /// <param name="item"></param>
        /// <remarks></remarks>
        protected void AddResponse(string item) {
            _response.Add(item);
        }
        #endregion

        #region  Process 
        /// <summary>
        /// Processes the specified response line.
        /// </summary>
        /// <param name="response"></param>
        /// <remarks></remarks>
        public abstract void Process(string response);

        /// <summary>
        /// Checks the response for timeouts and error values.
        /// </summary>
        /// <remarks>This method will be called on the thread that called the public method on <see cref="TcpSoundbridgeClient"/>.</remarks>
        public abstract void PostProcess();
        #endregion
    }
}
using System.Threading;

namespace Pixa.Soundbridge.Library
{

    /// <summary>
/// Processes responses for transacted RCP methods.
/// </summary>
/// <remarks></remarks>
    internal class TransactedResponseProcessor : ResponseProcessorBase
    {
        private TransactionStatus _status = TransactionStatus.Pending;

        /// <summary>
    /// Instantiates a new instance of ResponseProcessorBase for the specified SoundbridgeClient and EventWaitHandle.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="waitHandle"></param>
    /// <remarks></remarks>
        public TransactedResponseProcessor(TcpSoundbridgeClient client, string command, EventWaitHandle waitHandle) : base(client, command, waitHandle)
        {
        }

        /// <summary>
    /// Gets the status of the transaction.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks>If the transaction was not initiated, this usually indicates an error.</remarks>
        public TransactionStatus Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
    /// Processes the specified response line.
    /// </summary>
    /// <param name="response"></param>
    /// <remarks></remarks>
        public override void Process(string response)
        {
            if (response == "TransactionInitiated")
            {
                _status = TransactionStatus.Initiated;
                return;
            }

            if (response == "TransactionComplete")
            {
                _status = TransactionStatus.Complete;
                WaitHandle.Set();
                return;
            }

            if (response == "TransactionCanceled")
            {
                _status = TransactionStatus.Canceled;
                WaitHandle.Set();
                return;
            }

            if (ResponseCount == 0)
            {
                if (response == "StatusAwaitingReply")
                {
                    Client.OnAwaitingReply(Command);
                    return;
                }

                if (response == "StatusSendingRequest")
                {
                    Client.OnSendingRequest(Command);
                    return;
                }

                if (response.StartsWith("StatusReceivingData"))
                {
                    if (response.Contains(":"))
                    {
                        response = response.Substring(21);
                        if (response.Contains("/"))
                        {
                            var parts = response.Split('/');
                            int progress;
                            int total;
                            if (int.TryParse(parts[0], out progress) & int.TryParse(parts[1], out total))
                            {
                                Client.OnReceivingData(Command, progress, total);
                            }
                            else
                            {
                                Client.OnReceivingData(Command);
                            }
                        }
                        else
                        {
                            int progress;
                            if (int.TryParse(response, out progress))
                            {
                                Client.OnReceivingData(Command, progress);
                            }
                            else
                            {
                                Client.OnReceivingData(Command);
                            }
                        }
                    }
                    else
                    {
                        Client.OnReceivingData(Command);
                    }
                }
            }

            if (response.StartsWith("ListResultSize") | response == "ListResultEnd")
                return;
            AddResponse(response);
            if (_status == TransactionStatus.Pending)
            {
                WaitHandle.Set();
                return;
            }
        }

        public override void PostProcess()
        {
            if (_status != TransactionStatus.Complete & _status != TransactionStatus.Canceled & ResponseCount == 0)
                ExceptionHelper.ThrowCommandTimeout(Command);
        }
    }
}
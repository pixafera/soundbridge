namespace Pixa.Soundbridge.Client
{
    public class RcpCommandReceivingProgressEventArgs : RcpCommandProgressEventArgs
    {
        private int _progress;
        private int _total;

        public RcpCommandReceivingProgressEventArgs(string command) : this(command, -1)
        {
        }

        public RcpCommandReceivingProgressEventArgs(string command, int progress) : this(command, progress, -1)
        {
        }

        public RcpCommandReceivingProgressEventArgs(string command, int progress, int total) : base(command)
        {
            _progress = progress;
            _total = total;
        }

        /// <summary>
        /// Gets the progress of the transaction
        /// </summary>
        /// <value></value>
        /// <returns>The progress of the transaction, or -1 if this data was not sent.</returns>
        /// <remarks></remarks>
        public int Progress
        {
            get
            {
                return _progress;
            }
        }

        /// <summary>
        /// Gets the total size of the transaction being executed.
        /// </summary>
        /// <value></value>
        /// <returns>The total size of the transaction being executed, or -1 if this data was not sent.</returns>
        /// <remarks></remarks>
        public int Total
        {
            get
            {
                return _total;
            }
        }
    }
}
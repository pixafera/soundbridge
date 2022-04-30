using System;

namespace Pixa.Soundbridge.Library
{
    public class RcpCommandProgressEventArgs : EventArgs
    {
        private string _command;

        public RcpCommandProgressEventArgs(string command)
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
    }
}
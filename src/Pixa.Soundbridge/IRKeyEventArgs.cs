namespace Pixa.Soundbridge
{
    public class IRKeyEventArgs : SoundbridgeEventArgs
    {
        private IRCommand _command;
        private bool _isHandled = false;

        public IRKeyEventArgs(Soundbridge soundbridge, IRCommand command) : base(soundbridge)
        {
            _command = command;
        }

        public IRCommand Command
        {
            get
            {
                return _command;
            }
        }

        public bool IsHandled
        {
            get
            {
                return _isHandled;
            }
        }

        public void Handle()
        {
            _isHandled = true;
        }
    }
}
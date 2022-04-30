
namespace Pixa.Soundbridge.Library
{
    public class DisplayUpdateEventArgs : SoundbridgeEventArgs
    {
        private int _change;

        public DisplayUpdateEventArgs(Soundbridge soundbridge, int change) : base(soundbridge)
        {
            _change = change;
        }

        public int Change
        {
            get
            {
                return _change;
            }
        }
    }
}
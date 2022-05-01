using System;

namespace Pixa.Soundbridge
{
    public class SoundbridgeEventArgs : EventArgs
    {
        private Soundbridge _soundbridge;

        public SoundbridgeEventArgs(Soundbridge soundbridge)
        {
            _soundbridge = soundbridge;
        }

        public Soundbridge Soundbridge
        {
            get
            {
                return _soundbridge;
            }
        }
    }
}
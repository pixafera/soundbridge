using Pixa.Soundbridge.Client;
using System;

namespace Pixa.Soundbridge
{
    public class WirelessNetwork : SoundbridgeListObject
    {
        private bool _connected;
        private bool _selected;

        public event EventHandler SelectedChanged;

        #region  Classes 
        public class SignalQuality
        {
            private DateTime _time;
            private int _quality;
            private int _signal;
            private int _noise;

            public SignalQuality(int quality, int signal, int noise)
            {
                _time = DateTime.Now;
                _quality = quality;
                _signal = signal;
                _noise = noise;
            }

            public int Noise
            {
                get
                {
                    return _noise;
                }
            }

            public int Quality
            {
                get
                {
                    return _quality;
                }
            }

            public int Signal
            {
                get
                {
                    return _signal;
                }
            }

            public DateTime Time
            {
                get
                {
                    return _time;
                }
            }
        }
        #endregion

        internal WirelessNetwork(ISoundbridgeClient client, int index, string name, bool connected, bool selected) : base((Soundbridge)client, index, name)
        {
        }

        public bool Connected
        {
            get
            {
                return _connected;
            }
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }

            internal set
            {
                _selected = value;
            }
        }

        public SignalQuality GetSignalQuality()
        {
            if (!Connected)
                throw new InvalidOperationException("Can't get the signal quality when the soundbridge isn't connected to the network");
            var quality = default(int);
            var signal = default(int);
            var noise = default(int);
            foreach (string s in Client.GetWiFiSignalQuality())
            {
                if (s == "OK")
                    continue;
                int iNumberLeft = s.Length - 1;
                while (char.IsNumber(s, iNumberLeft) | s[iNumberLeft] == '-')
                    iNumberLeft -= 1;
                int sNumber = int.Parse(s.Substring(iNumberLeft + 1));
                int i;
                if (int.TryParse(sNumber.ToString(), out i))
                {
                    if (s.StartsWith("quality"))
                    {
                        quality = i;
                    }
                    else if (s.StartsWith("signal"))
                    {
                        signal = i;
                    }
                    else if (s.StartsWith("noise"))
                    {
                        noise = i;
                    }
                }
            }

            return new SignalQuality(quality, signal, noise);
        }

        public void Select(string password)
        {
            string r;
            r = Client.SetWiFiNetworkSelection(Index);
            if (r != "OK")
                ExceptionHelper.ThrowCommandReturnError("SetWiFiNetworkSelection", r);
            if (!string.IsNullOrEmpty(password))
            {
                r = Client.SetWiFiPassword(password);
                if (r != "OK")
                    ExceptionHelper.ThrowCommandReturnError("SetWiFiPassword", r);
            }
        }
    }
}
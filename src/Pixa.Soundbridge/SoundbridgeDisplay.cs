using System;

namespace Pixa.Soundbridge {
    public sealed class SoundbridgeDisplay : SoundbridgeObject {
        private Soundbridge _sb;
        private EventHandler<DisplayUpdateEventArgs> _update;

        public event EventHandler<DisplayUpdateEventArgs> Update {
            add {
                _update = (EventHandler<DisplayUpdateEventArgs>)Delegate.Combine(_update, value);
                if (_update is object)
                    Client.DisplayUpdateEventSubscribe();
            }

            remove {
                _update = (EventHandler<DisplayUpdateEventArgs>)Delegate.Remove(_update, value);
                if (_update is null)
                    Client.DisplayUpdateEventUnsubscribe();
            }
        }

        void OnUpdate(object sender, SoundbridgeEventArgs e) {
            _update(sender, (DisplayUpdateEventArgs)e);
        }

        internal SoundbridgeDisplay(Soundbridge sb) : base(sb) {
            _sb = sb;
            Client.DisplayUpdate += Client_DisplayUpdate;
            Client.DisplayUpdateEventSubscribe();
        }

        public bool SupportsVisualizers {
            get {
                return Client.GetVisualizer(false) != "ErrorUnsupported";
            }
        }

        public VisualizerMode VisualizerMode {
            get {
                return StringToVisualizerMode(Client.GetVisualizerMode());
            }

            set {
                Client.SetVisualizerMode(VisualizerModeToString(value));
            }
        }

        public string VisualizerName {
            get {
                string r = Client.GetVisualizer(false);
                if (r == "ErrorUnsupported")
                    ExceptionHelper.ThrowCommandReturnError("GetVisualizer", r);
                return r;
            }

            set {
                string r = Client.SetVisualizer(value);
                if (r != "OK")
                    ExceptionHelper.ThrowCommandReturnError("SetVisualizer", r);
            }
        }

        public string VisualizerFriendlyName {
            get {
                string r = Client.GetVisualizer(true);
                if (r == "ErrorUnsupported")
                    ExceptionHelper.ThrowCommandReturnError("GetVisualizer", r);
                return r;
            }
        }

        public byte[] GetVizDataVU() {
            string r = Client.GetVizDataVU();
            if (r == "OK")
                return new byte[] { };
            return Soundbridge.ResponseToByteArray(r);
        }

        public byte[] GetVizDataFreq() {
            string r = Client.GetVizDataFreq();
            if (r == "OK")
                return new byte[] { };
            return Soundbridge.ResponseToByteArray(r);
        }

        public byte[] GetVizDataScope() {
            string r = Client.GetVizDataScope();
            if (r == "OK")
                return new byte[] { };
            return Soundbridge.ResponseToByteArray(r);
        }

        public bool GetDisplayData(ref string textualData, byte[] byteData) {
            var isByte = default(bool);
            string r = Client.GetDisplayData(ref isByte);
            if (isByte) {
                byteData = Soundbridge.ResponseToByteArray(r);
                return true;
            } else {
                textualData = r;
                return false;
            }
        }

        private string VisualizerModeToString(VisualizerMode value) {
            switch (value) {
                case VisualizerMode.Full: {
                        return "full";
                    }

                case VisualizerMode.Off: {
                        return "off";
                    }

                case VisualizerMode.Partial: {
                        return "partial";
                    }

                default: {
                        return "";
                    }
            }
        }

        private VisualizerMode StringToVisualizerMode(string value) {
            switch (value ?? "") {
                case "full": {
                        return VisualizerMode.Full;
                    }

                case "off": {
                        return VisualizerMode.Off;
                    }

                case "partial": {
                        return VisualizerMode.Partial;
                    }

                default: {
                        return 0;
                    }
            }
        }

        private void Client_DisplayUpdate(string data) {
            int iData;
            if (int.TryParse(data, out iData))
                OnUpdate(this, new DisplayUpdateEventArgs(_sb, iData));
        }
    }
}
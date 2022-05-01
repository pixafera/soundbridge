using Pixa.Soundbridge.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pixa.Soundbridge {
    public class SoundbridgeOptions : SoundbridgeObject, IDictionary<string, string> {
        private ReadOnlyCollection<string> _optionKeys;

        internal SoundbridgeOptions(SoundbridgeObject obj) : base(obj) {
            var l = new List<string>();
            l.Add("bootmode");
            l.Add("standbyMode");
            l.Add("outputMultichannel");
            l.Add("reventToNowPlaying");
            l.Add("scrollLongInfo");
            l.Add("displayComposer");
            l.Add("skipUnchecked");
            l.Add("wmaThreshold");
            _optionKeys = new ReadOnlyCollection<string>(l);
        }

        internal SoundbridgeOptions(ISoundbridgeClient client) : base(client) {
        }

        #region  Unsupported 

        private void Add(KeyValuePair<string, string> item) {
            throw new NotSupportedException("Cannot change elements in this dictionary");
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) => Add(item);

        private void Clear() {
            throw new NotSupportedException("Cannot change elements in this dictionary");
        }

        void ICollection<KeyValuePair<string, string>>.Clear() => Clear();

        private bool Remove(KeyValuePair<string, string> item) {
            throw new NotSupportedException("Cannot change elements in this dictionary");
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) => Remove(item);

        private void Add1(string key, string value) {
            throw new NotSupportedException("Cannot change elements in this dictionary");
        }

        void IDictionary<string, string>.Add(string key, string value) => Add1(key, value);

        private bool Remove1(string key) {
            throw new NotSupportedException("Cannot change elements in this dictionary");
        }

        bool IDictionary<string, string>.Remove(string key) => Remove1(key);

        #endregion

        #region  Dictionary 
        public bool Contains(KeyValuePair<string, string> item) {
            return ContainsKey(item.Key) && (this[item.Key] ?? "") == (item.Value ?? "");
        }

        public bool ContainsKey(string key) {
            return _optionKeys.Contains(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
            for (int i = 0, loopTo = _optionKeys.Count - 1; i <= loopTo; i++)
                array[arrayIndex + i] = new KeyValuePair<string, string>(_optionKeys[i], this[_optionKeys[i]]);
        }

        public int Count {
            get {
                return _optionKeys.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return true;
            }
        }

        public string this[string key] {
            get {
                string r = Client.GetOption(key);
                if (r == "ParameterError" | r == "GenericError" | r == "ErrorUnsupported")
                    ExceptionHelper.ThrowCommandReturnError("GetOption", r);
                return r;
            }

            set {
                string r = Client.SetOption(key, value);
                if (r != "OK")
                    ExceptionHelper.ThrowCommandReturnError("SetOption", r);
            }
        }

        public ICollection<string> Keys {
            get {
                return _optionKeys;
            }
        }

        public bool TryGetValue(string key, out string value) {
            try {
                value = this[key];
                return true;
            } catch (Exception ex) {
                value = null;
                return false;
            }
        }

        public ICollection<string> Values {
            get {
                var l = new List<string>();
                foreach (string s in _optionKeys)
                    l.Add(this[s]);
                return new ReadOnlyCollection<string>(l);
            }
        }
        #endregion

        #region  Enumerator 
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return new SoundbridgeOptionsEnumerator(this);
        }

        private IEnumerator GetNonGenericEnumerator() {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetNonGenericEnumerator();

        private class SoundbridgeOptionsEnumerator : IEnumerator<KeyValuePair<string, string>> {
            private SoundbridgeOptions _options;
            private int _index = 0;
            private KeyValuePair<string, string> _current;

            public SoundbridgeOptionsEnumerator(SoundbridgeOptions options) {
                _options = options;
            }

            public KeyValuePair<string, string> Current {
                get {
                    return _current;
                }
            }

            private object Current1 {
                get {
                    return Current;
                }
            }

            object IEnumerator.Current { get => Current1; }

            public bool MoveNext() {
                _index += 1;
                if (_index >= _options.Count)
                    return false;
                UpdateCurrent();
                return true;
            }

            public void Reset() {
                _index = 0;
            }

            private void UpdateCurrent() {
                string key = _options._optionKeys[_index];
                _current = new KeyValuePair<string, string>(key, _options[key]);
            }

            #region  IDisposable Support 
            private bool disposedValue = false;        // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing) {
                if (!disposedValue) {
                    if (disposing) {
                        // TODO: free other state (managed objects).
                    }

                    // TODO: free your own state (unmanaged objects).
                    // TODO: set large fields to null.
                }

                disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose() {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion

        }
        #endregion

        #region  Option Properties 
        public BootMode BootMode {
            get {
                switch (this["bootMode"] ?? "") {
                    case "lastState": {
                            return BootMode.LastSource;
                        }

                    case "standby": {
                            return BootMode.Standby;
                        }

                    case "lastSource": {
                            return BootMode.LastSource;
                        }

                    case "serverList": {
                            return BootMode.ServerList;
                        }
                }

                return default;
            }

            set {
                string sValue;
                switch (value) {
                    case BootMode.LastSource: {
                            sValue = "lastSource";
                            break;
                        }

                    case BootMode.LastState: {
                            sValue = "lastState";
                            break;
                        }

                    case BootMode.ServerList: {
                            sValue = "serverList";
                            break;
                        }

                    case BootMode.Standby: {
                            sValue = "standby";
                            break;
                        }
                }

                this["bootMode"] = ((int)value).ToString();
            }
        }

        public bool ShowClockInStandby {
            get {
                return this["standbyMode"] == "clock";
            }

            set {
                this["standbyMode"] = value ? "clock" : "screenOff";
            }
        }

        public bool OutputMultichannel {
            get {
                return this["outputMultichannel"] == "1";
            }

            set {
                this["outputMultichannel"] = (value ? 1 : 0).ToString();
            }
        }

        public bool RevertToNowPlaying {
            get {
                return this["revertToNowPlaying"] == "1";
            }

            set {
                this["revertToNowPlaying"] = (value ? 1 : 0).ToString();
            }
        }

        public bool ScrollLongInfo {
            get {
                return this["scrollLongInfo"] == "1";
            }

            set {
                this["scrollLongInfo"] = (value ? 1 : 0).ToString();
            }
        }

        public bool DisplayComposer {
            get {
                return this["displayComposer"] == "1";
            }

            set {
                this["displayComposer"] = (value ? 1 : 0).ToString();
            }
        }

        public bool SkipUnchecked {
            get {
                return this["skipUnchecked"] == "1";
            }

            set {
                this["skipUnchecked"] = (value ? 1 : 0).ToString();
            }
        }

        public int WmaThreshold {
            get {
                return int.Parse(this["wmaThreshold"]);
            }

            set {
                this["wmaThreshold"] = value.ToString();
            }
        }
        #endregion

    }
}
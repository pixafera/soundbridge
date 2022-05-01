using System;
using System.Collections;
using System.Collections.Generic;

namespace Pixa.Soundbridge {
    public class NetworkAdapterCollection : SoundbridgeObject, ICollection<NetworkAdapter> {
        private NetworkAdapter _ethernet;
        private NetworkAdapter _wireless;
        private NetworkAdapter _loopback;
        private Soundbridge _soundbridge;

        internal NetworkAdapterCollection(Soundbridge sb) : base(sb) {
            _soundbridge = sb;
            _loopback = new NetworkAdapter(sb, NetworkAdapterType.Loopback);
            _ethernet = new NetworkAdapter(sb, NetworkAdapterType.Ethernet);
            _wireless = new NetworkAdapter(sb, NetworkAdapterType.Wireless);
        }

        public Soundbridge Soundbridge {
            get {
                return _soundbridge;
            }
        }

        #region  Item 
        public NetworkAdapter this[NetworkAdapterType adapter] {
            get {
                switch (adapter) {
                    case NetworkAdapterType.Ethernet: {
                            return _ethernet;
                        }

                    case NetworkAdapterType.Loopback: {
                            return _loopback;
                        }

                    case NetworkAdapterType.Wireless: {
                            return _wireless;
                        }

                    default: {
                            throw new ArgumentOutOfRangeException("Only valid NetworkAdapterTypes are accepted");
                        }
                }
            }
        }
        #endregion

        #region  Wireless 
        private DateTime _lastWifiNetworkRefresh;
        private WirelessNetworkCollection _wirelessNetworks;

        public WirelessNetworkCollection WirelessNetworks {
            get {
                if (!_wirelessNetworks.IsActive || (DateTime.Now - _lastWifiNetworkRefresh).TotalSeconds > 30d) {
                    if (_wirelessNetworks is object) {
                        foreach (WirelessNetwork wn in _wirelessNetworks)
                            wn.SelectedChanged -= WirelessNetwork_SelectedChanged;
                    }

                    var networks = Client.ListWiFiNetworks();
                    if (networks.Length > 0 && (networks[0] == "ErrorInitialSetupRequired" || networks[0] == "ErrorNoWiFiInterfaceFound" || networks[0] == "ErrorWiFiInterfaceDisabled"))
                        ExceptionHelper.ThrowCommandReturnError("ListWiFiNetworks", networks[0]);
                    _wirelessNetworks = new WirelessNetworkCollection(Soundbridge);
                    string selectedNetwork = Client.GetWiFiNetworkSelection();
                    string connectedNetwork = Client.GetConnectedWiFiNetwork();
                    for (int i = 0, loopTo = networks.Length - 1; i <= loopTo; i++) {
                        string network = networks[i];
                        _wirelessNetworks.Add(new WirelessNetwork(Client, i, network, (network ?? "") == (connectedNetwork ?? ""), (network ?? "") == (selectedNetwork ?? "")));
                    }

                    Soundbridge.ActiveList = _wirelessNetworks;
                }

                return _wirelessNetworks;
            }
        }

        private void WirelessNetwork_SelectedChanged(object sender, EventArgs e) {
            foreach (WirelessNetwork wn in _wirelessNetworks) {
                if (!ReferenceEquals(wn, sender))
                    wn.Selected = false;
            }
        }
        #endregion

        #region  Not Supported 
        private void Add(NetworkAdapter item) {
            throw new NotSupportedException("Cannot change this collection");
        }

        void ICollection<NetworkAdapter>.Add(NetworkAdapter item) => Add(item);

        public void Clear() {
            throw new NotSupportedException("Cannot change this collection");
        }

        public bool Remove(NetworkAdapter item) {
            throw new NotSupportedException("Connect change this collection");
        }
        #endregion

        #region  ICollection 
        public bool Contains(NetworkAdapter item) {
            return item is object && ReferenceEquals(item, _ethernet) | ReferenceEquals(item, _wireless) | ReferenceEquals(item, _loopback);
        }

        public void CopyTo(NetworkAdapter[] array, int arrayIndex) {
            int i = 0;
            if (_loopback is object) {
                array[arrayIndex + i] = _loopback;
                i += 1;
            }

            if (_ethernet is object) {
                array[arrayIndex + i] = _ethernet;
                i += 1;
            }

            if (_wireless is object) {
                array[arrayIndex + i] = _wireless;
                i += 1;
            }
        }

        public int Count {
            get {
                var i = default(int);
                if (_loopback is object)
                    i += 1;
                if (_ethernet is object)
                    i += 1;
                if (_wireless is object)
                    i += 1;
                return i;
            }
        }

        public bool IsReadOnly {
            get {
                return true;
            }
        }

        public IEnumerator<NetworkAdapter> GetGenericEnumerator() {
            return new NetworkAdapterCollectionEnumerator(this);
        }

        IEnumerator<NetworkAdapter> IEnumerable<NetworkAdapter>.GetEnumerator() => GetGenericEnumerator();

        private IEnumerator GetEnumerator() {
            return GetGenericEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region  IEnumerator 
        private class NetworkAdapterCollectionEnumerator : IEnumerator<NetworkAdapter> {
            private NetworkAdapterCollection _collection;
            private NetworkAdapterType _type;

            public NetworkAdapterCollectionEnumerator(NetworkAdapterCollection c) {
                _collection = c;
            }

            #region  Generic 

            public NetworkAdapter Current {
                get {
                    return _collection[_type];
                }
            }

            #endregion

            public object CurrentObject {
                get {
                    return Current;
                }
            }

            object IEnumerator.Current { get => CurrentObject; }

            public bool MoveNext() {
                _type = _type + 1;
                return (int)_type < 3;
            }

            public void Reset() {
                _type = 0;
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

    }
}
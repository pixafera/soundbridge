using Pixa.Soundbridge.Client;
using System;

namespace Pixa.Soundbridge {
    public abstract class SoundbridgeObject : IDisposable {
        private ISoundbridgeClient _client;

        protected SoundbridgeObject(SoundbridgeObject obj) : this(obj.Client) {
        }

        protected SoundbridgeObject(ISoundbridgeClient client) {
            _client = client;
        }

        protected internal ISoundbridgeClient Client {
            get {
                return _client;
            }
        }

        #region  IDisposable Support 
        private bool _disposed = false;      // To detect redundant calls

        public bool Disposed {
            get {
                return _disposed;
            }
        }

        public virtual bool ShouldCacheDispose {
            get {
                return true;
            }
        }

        // IDisposable
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _client = null;
                }

                // TODO: free your own state (unmanaged objects).
                // TODO: set large fields to null.
            }

            _disposed = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
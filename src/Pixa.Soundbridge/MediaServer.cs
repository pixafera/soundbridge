using Pixa.Soundbridge.Client;
using System;

namespace Pixa.Soundbridge
{
    /// <summary>
    /// Represents a media streaming server that a <see cref="Soundbridge"/> can
    /// connect to.
    /// </summary>
    /// <remarks></remarks>
    public class MediaServer : SoundbridgeListObject
    {
        private MediaServerAvailability _availability;
        private Soundbridge _sb;
        private MediaServerType _type;
        private MediaServerQuerySupport _querySupport;
        private bool _containers;
        private bool _playlists;
        private bool _partialResults;

        /// <summary>
        /// Initialises a new instance of <see cref="MediaServer"/> from information
        /// gleaned from the <see cref="ISoundbridgeClient.ListServers"/> command.
        /// </summary>
        internal MediaServer(Soundbridge sb, MediaServerAvailability availability, MediaServerType type, string name, int index) : base(sb, index, name)
        {
            _sb = sb;
            _availability = availability;
            _type = type;
            _container = new MediaContainer(this, 0, "");
        }

        /// <summary>
        /// Gets a value to determine whether the <see cref="MediaServer"/> is the
        /// active server that the <see cref="Soundbridge"/> is connected to.
        /// </summary>
        /// <value>True if the <see cref="MediaServer"/> is the active server;
        /// otherwise, false.</value>
        public bool Connected
        {
            get
            {
                return ReferenceEquals(Soundbridge.ConnectedServer, this);
            }
        }

        /// <summary>
        /// Gets the availability of the media server.
        /// </summary>
        public MediaServerAvailability Availability
        {
            get
            {
                return _availability;
            }
        }

        /// <summary>
        /// Gets the <see cref="Soundbridge"/> that received the <see cref="MediaServer"/>
        /// information.
        /// </summary>
        public Soundbridge Soundbridge
        {
            get
            {
                return _sb;
            }
        }

        /// <summary>
        /// Gets the media server's <see cref="MediaServerType"/>.
        /// </summary>
        public MediaServerType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Connects the <see cref="Soundbridge"/> to the <see cref="MediaServer"/>.
        /// </summary>
        /// <remarks></remarks>
        public void Connect()
        {
            if (Soundbridge.ConnectedServer is object)
                throw new InvalidOperationException("Can't connect to a server when there's already one connected.");
            int tries = 0;
            while (tries < 2)
            {
                string r = Client.ServerConnect(Index);
                bool exitWhile = false;
                switch (r ?? "")
                {
                    case "Connected":
                        {
                            exitWhile = true;
                            break;
                        }

                    case "ConnectionFailedAlreadyConnected":
                    case "GenericError":
                        {
                            Client.GetConnectedServer();
                            Client.ServerDisconnect();
                            break;
                        }

                    default:
                        {
                            ExceptionHelper.ThrowCommandReturnError("ServerConnect", r);
                            break;
                        }
                }

                if (exitWhile)
                {
                    break;
                }
            }

            OnAfterConnect();
        }

        /// <summary>
        /// Connects the <see cref="Soundbridge"/> to the <see cref="MediaServer"/>
        /// using the specified password.
        /// </summary>
        /// <param name="password">The password to use to connect to the media
        /// server.</param>
        /// <remarks></remarks>
        public void Connect(string password)
        {
            string r = Client.SetServerConnectPassword(password);
            if (r != "OK")
                ExceptionHelper.ThrowCommandReturnError("SetServerConnectPassword", r);
            Connect();
        }

        /// <summary>
        /// Connects the <see cref="Soundbridge"/> to the <see cref="MediaServer"/>
        /// allowing the Soundbridge's UI to prompt for the password to the media
        /// server if one is needed.
        /// </summary>
        /// <remarks></remarks>
        public void LaunchUi()
        {
            string r = Client.ServerLaunchUI(Index);
            if (r != "OK")
                ExceptionHelper.ThrowCommandReturnError("ServerLaunchUI", r);
            OnAfterConnect();
        }

        /// <summary>
        /// Sets this <see cref="MediaServer"/> as the active server.
        /// </summary>
        /// <remarks></remarks>
        private void OnAfterConnect()
        {
            Soundbridge.ConnectedServer = this;
            GetCapabilities();
        }

        /// <summary>
        /// Updates the <see cref="MediaServer"/> with its capabilities.
        /// </summary>
        private void GetCapabilities()
        {
            var info = Client.ServerGetCapabilities();
            if (info.Length == 1)
                ExceptionHelper.ThrowCommandReturnError("ServerGetCapabilities", info[0]);
            _querySupport = QuerySupportToMediaServerQuerySupport(info[0].Substring(14));
            _containers = info[1].Substring(12) == "yes";
            _playlists = info[2].Substring(11) == "yes";
            _partialResults = info[3].Substring(16) == "yes";
        }

        /// <summary>
        /// Converts a server type received from the <see cref="Soundbridge"/> to a
        /// <see cref="MediaServerType"/> value.
        /// </summary>
        private MediaServerType ActiveServerTypeToMediaServerType(string value)
        {
            switch (value ?? "")
            {
                case "daap":
                    {
                        return MediaServerType.Daap;
                    }

                case "upnp":
                    {
                        return MediaServerType.Upnp;
                    }

                case "rsp":
                    {
                        return MediaServerType.Rsp;
                    }

                case "slim":
                    {
                        return MediaServerType.Slim;
                    }

                case "radio":
                    {
                        return MediaServerType.Radio;
                    }

                case "flash":
                    {
                        return MediaServerType.Flash;
                    }

                case "linein":
                    {
                        return MediaServerType.LineIn;
                    }

                case "am":
                    {
                        return MediaServerType.AM;
                    }

                case "fm":
                    {
                        return MediaServerType.FM;
                    }

                default:
                    {
                        return (MediaServerType)(-1);
                    }
            }
        }

        /// <summary>
        /// Converts a string received from the <see cref="Soundbridge"/> to a <see cref="MediaServerQuerySupport"/>
        /// value.
        /// </summary>
        private MediaServerQuerySupport QuerySupportToMediaServerQuerySupport(string value)
        {
            switch (value ?? "")
            {
                case "None":
                    {
                        return MediaServerQuerySupport.None;
                    }

                case "Songs":
                    {
                        return MediaServerQuerySupport.Songs;
                    }

                case "Basic":
                    {
                        return MediaServerQuerySupport.Basic;
                    }

                case "Partial":
                    {
                        return MediaServerQuerySupport.Partial;
                    }

                default:
                    {
                        return (MediaServerQuerySupport)(-1);
                    }
            }
        }

        public override bool ShouldCacheDispose
        {
            get
            {
                return !Connected;
            }
        }

        private MediaContainer _container;

        public MediaContainer Container
        {
            get
            {
                return _container;
            }
        }
    }
}
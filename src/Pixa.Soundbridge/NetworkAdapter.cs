using Pixa.Soundbridge.Client;
using System.Net;
using System.Net.NetworkInformation;

namespace Pixa.Soundbridge
{
    public class NetworkAdapter : SoundbridgeObject
    {
        private NetworkAdapterStatus _status;
        private NetworkAdapterType _type;
        private PhysicalAddress _macAddress;
        private IPAddress _ipAddress;

        internal NetworkAdapter(SoundbridgeObject obj, NetworkAdapterType type) : base(obj)
        {
            Initialize(type);
        }

        internal NetworkAdapter(ISoundbridgeClient client, NetworkAdapterType type) : base(client)
        {
            Initialize(type);
        }

        private void Initialize(NetworkAdapterType type)
        {
            string code = NetworkAdapterTypeToCode(type);
            _status = StringToNetworkAdapterStatus(Client.GetLinkStatus(code));
            if (_status != NetworkAdapterStatus.NotFound)
            {
                _ipAddress = IPAddress.Parse(Client.GetIPAddress(code));
                _macAddress = PhysicalAddress.Parse(Client.GetMacAddress(code));
            }

            _type = type;
        }

        public IPAddress IPAddress
        {
            get
            {
                return _ipAddress;
            }
        }

        public PhysicalAddress MacAddress
        {
            get
            {
                return _macAddress;
            }
        }

        public NetworkAdapterStatus Status
        {
            get
            {
                return _status;
            }
        }

        public NetworkAdapterType Type
        {
            get
            {
                return _type;
            }
        }

        private string NetworkAdapterTypeToCode(NetworkAdapterType type)
        {
            switch (type)
            {
                case NetworkAdapterType.Ethernet:
                    {
                        return "enet";
                    }

                case NetworkAdapterType.Loopback:
                    {
                        return "loop";
                    }

                case NetworkAdapterType.Wireless:
                    {
                        return "wlan";
                    }

                default:
                    {
                        return "";
                    }
            }
        }

        private NetworkAdapterStatus StringToNetworkAdapterStatus(string value)
        {
            switch (value ?? "")
            {
                case "Link":
                    {
                        return NetworkAdapterStatus.Link;
                    }

                case "NoLink":
                    {
                        return NetworkAdapterStatus.NoLink;
                    }

                case "ErrorNotFound":
                    {
                        return NetworkAdapterStatus.NotFound;
                    }

                default:
                    {
                        ExceptionHelper.ThrowCommandReturnError("GetLinkStatus", value);
                        break;
                    }
            }

            return default;
        }
    }
}
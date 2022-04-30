using System.Net;

namespace Pixa.Soundbridge.Library
{
    public static class SoundbridgeFactory
    {
        public static Soundbridge CreateFromTcp(IPEndPoint endPoint)
        {
            return new Soundbridge(new TcpSoundbridgeClient(endPoint));
        }

        public static Soundbridge CreateFromTcp(string hostname)
        {
            return new Soundbridge(new TcpSoundbridgeClient(hostname));
        }

        public static Soundbridge CreateFromTcp(string hostname, int port)
        {
            return new Soundbridge(new TcpSoundbridgeClient(hostname, port));
        }
    }
}
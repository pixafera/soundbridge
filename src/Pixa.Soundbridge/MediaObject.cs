namespace Pixa.Soundbridge
{
    public class MediaObject : SoundbridgeListObject
    {
        private MediaServer _server;

        internal MediaObject(MediaServer server, int index, string name) : base(server.Soundbridge, index, name)
        {
            _server = server;
        }

        public MediaServer Server
        {
            get
            {
                return _server;
            }
        }
    }
}
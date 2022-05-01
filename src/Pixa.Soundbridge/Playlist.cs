namespace Pixa.Soundbridge {
    public class Playlist : MediaObject {
        private SongCollection _songs;
        private int _index;

        internal Playlist(MediaServer server, int index, string name) : base(server, index, name) {
            _index = index;
        }

        public SongCollection Songs {
            get {
                if (!Server.Soundbridge.ActiveList.Contains(this))
                    ExceptionHelper.ThrowObjectNotActive();
                if (_songs is null || !_songs.IsActive) {
                    _songs = new SongCollection(Server.Soundbridge);
                    var songNames = Client.ListPlaylistSongs(Index);
                    if (songNames.Length == 1 && songNames[0] == "ErrorDisconnected" | songNames[0] == "GenericError")
                        ExceptionHelper.ThrowCommandReturnError("ListSongs", songNames[0]);
                    for (int i = 0, loopTo = songNames.Length - 1; i <= loopTo; i++)
                        _songs.Add(new Song(Server, int.Parse(songNames[i]), i.ToString()));
                }

                return _songs;
            }
        }
    }
}
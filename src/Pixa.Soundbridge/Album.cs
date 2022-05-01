namespace Pixa.Soundbridge {
    public class Album : MediaObject {
        private Artist _artist;
        private SongCollection _songs;

        internal Album(MediaServer server, int index, string name) : this(server, index, name, null) {
        }

        internal Album(MediaServer server, int index, string name, Artist artist) : base(server, index, name) {
            _artist = artist;
        }

        public SongCollection Songs {
            get {
                if (_songs is null || !_songs.IsActive) {
                    _songs = new SongCollection(Server.Soundbridge);
                    string r = Client.SetBrowseFilterAlbum(Name);
                    if (r != "OK")
                        ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterAlbum", r);
                    if (_artist is object) {
                        r = Client.SetBrowseFilterArtist(_artist.Name);
                        if (r != "OK")
                            ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterArtist", r);
                    }

                    var songNames = Client.ListSongs();
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
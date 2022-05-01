namespace Pixa.Soundbridge
{
    public class Artist : MediaObject
    {
        private AlbumCollection _albums;
        private SongCollection _songs;

        internal Artist(MediaServer server, int index, string name) : base(server, index, name)
        {
        }

        public AlbumCollection Albums
        {
            get
            {
                if (_albums is null)
                {
                    _albums = new AlbumCollection(Server.Soundbridge);
                    string r = Client.SetBrowseFilterArtist(Name);
                    if (r != "OK")
                        ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterArtist", r);
                    var albumNames = Client.ListAlbums();
                    if (albumNames.Length == 1 && albumNames[0] == "ErrorDisconnected" | albumNames[0] == "GenericError")
                        ExceptionHelper.ThrowCommandReturnError("ListAlbums", albumNames[0]);
                    int i = 0;
                    foreach (string s in albumNames)
                    {
                        _albums.Add(new Album(Server, i, s));
                        i += 1;
                    }
                }

                return _albums;
            }
        }

        public SongCollection Songs
        {
            get
            {
                if (_songs is null || !_songs.IsActive)
                {
                    _songs = new SongCollection(Server.Soundbridge);
                    string r = Client.SetBrowseFilterArtist(Name);
                    if (r != "OK")
                        ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterArtist", r);
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

    public class ArtistCollection : SoundbridgeObjectCollection<Artist>
    {
        internal ArtistCollection(Soundbridge sb) : base(sb)
        {
        }
    }
}
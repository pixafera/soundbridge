namespace Pixa.Soundbridge
{
    public class Genre : MediaObject
    {
        private SongCollection _songs;

        internal Genre(MediaServer server, int index, string name) : base(server, index, name)
        {
        }

        public SongCollection Songs
        {
            get
            {
                if (_songs is null || !_songs.IsActive)
                {
                    _songs = new SongCollection(Server.Soundbridge);
                    string r = Client.SetBrowseFilterGenre(Name);
                    if (r != "OK")
                        ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterGenre", r);
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
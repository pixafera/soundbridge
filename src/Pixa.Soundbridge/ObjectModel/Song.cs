using System;
using System.Collections.Generic;

namespace Pixa.Soundbridge.Library
{
    public class Song : MediaObject
    {
        private bool _hasInfo;
        private Dictionary<string, string> _info;

        internal Song(MediaServer server, int index, string name) : base(server, index, name)
        {
        }

        internal Song(MediaContainer parent, int index, string name) : base(parent.Server, index, name)
        {
        }

        public string Album
        {
            get
            {
                return GetSongInfo("album");
            }
        }

        public string Artist
        {
            get
            {
                return GetSongInfo("artist");
            }
        }

        public string Genre
        {
            get
            {
                return GetSongInfo("genre");
            }
        }

        public bool HasInfo
        {
            get
            {
                return _hasInfo;
            }
        }

        public string Id
        {
            get
            {
                return GetSongInfo("id");
            }
        }

        public string Status
        {
            get
            {
                return GetSongInfo("status");
            }
        }

        public string Title
        {
            get
            {
                return GetSongInfo("title");
            }
        }

        /// <summary>
   /// Gets the length of the track in milliseconds.
   /// </summary>
   /// <value>The length of the track in milliseconds.</value>
        public int TrackLength
        {
            get
            {
                return Int32.Parse(GetSongInfo("trackLengthMS"));
            }
        }

        /// <summary>
   /// Gets the track number
   /// </summary>
   /// <value>The length of the track in milliseconds.</value>
        public int TrackNumber
        {
            get
            {
                return Int32.Parse(GetSongInfo("trackNumber"));
            }
        }

        public string Url
        {
            get
            {
                return GetSongInfo("resource[0] url:");
            }
        }

        public string GetSongInfo(string key)
        {
            if (!HasInfo)
            {
                if (IsActive)
                {
                    var info = Client.GetSongInfo(Index);
                    _info = new Dictionary<string, string>();
                    foreach (string s in info)
                    {
                        var parts = s.Split(":".ToCharArray(), 2, StringSplitOptions.None);
                        _info.Add(parts[0], parts[1]);
                    }

                    _hasInfo = true;
                }
                else
                {
                    throw new InvalidOperationException("Can't retrieve SongInfo when the MediaObject is not in the active list.");
                }
            }

            if (_info.ContainsKey(key))
            {
                return _info[key];
            }
            else
            {
                return "";
            }
        }

        public void Play()
        {
            Play(false);
        }

        public void Play(bool excludeList)
        {
            if (IsActive)
            {
                if (excludeList)
                {
                    string r = Client.QueueAndPlayOne(Index);
                    if (r != "OK")
                    {
                        ExceptionHelper.ThrowCommandReturnError("QueueAndPlayOne", r);
                    }
                }
                else
                {
                    string r = Client.QueueAndPlay(Index);
                    if (r != "OK")
                    {
                        ExceptionHelper.ThrowCommandReturnError("QueueAndPlay", r);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Can't play a song that's not in the active list");
            }
        }

        public void InsertIntoNowPlaying(int targetIndex)
        {
            if (IsActive)
            {
                string r = Client.NowPlayingInsert(Index, targetIndex);
                if (r != "OK")
                {
                    ExceptionHelper.ThrowCommandReturnError("NowPlayingInsert", r);
                }
            }
            else
            {
                throw new InvalidOperationException("Can't play a song that's not in the active list");
            }
        }
    }

    public class SongCollection : SoundbridgeObjectCollection<Song>
    {
        internal SongCollection(Soundbridge sb) : base(sb)
        {
        }

        public void Play()
        {
            Play(0);
        }

        public void Play(int startingIndex)
        {
            if (IsActive)
            {
                string r = Soundbridge.Client.QueueAndPlay(startingIndex);
                if (r != "OK")
                {
                    ExceptionHelper.ThrowCommandReturnError("QueueAndPlay", r);
                }
            }
            else
            {
                throw new InvalidOperationException("Can't play a song list that's not the active list");
            }
        }

        public void InsertIntoNowPlaying(int targetIndex)
        {
            if (IsActive)
            {
                string r = Soundbridge.Client.NowPlayingInsert(targetIndex);
                if (r != "OK")
                {
                    ExceptionHelper.ThrowCommandReturnError("NowPlayingInsert", r);
                }
            }
            else
            {
                throw new InvalidOperationException("Can't play a song list that's not the active list");
            }
        }
    }
}
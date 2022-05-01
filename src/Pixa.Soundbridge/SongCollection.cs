using System;

namespace Pixa.Soundbridge
{
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
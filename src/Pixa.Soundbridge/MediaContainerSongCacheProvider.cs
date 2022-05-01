using System;

namespace Pixa.Soundbridge {
    internal class MediaContainerSongCacheProvider : SoundbridgeListCacheProvider<Song> {
        private bool _listIsSongs;

        public MediaContainerSongCacheProvider(Soundbridge soundbridge, MediaContainer parent) : base(soundbridge, parent) {
        }

        protected override SoundbridgeObjectCollection<Song> CreateCollection() {
            return new SongCollection(Soundbridge);
        }

        protected override Song CreateObject(string elementData, int index) {
            if (index == 0) {
                var l = Soundbridge.Client.GetSongInfo(index);
                _listIsSongs = Array.IndexOf(l, "format: unsupported") < 0;
            }

            if (_listIsSongs) {
                return new Song((MediaContainer)Parent, index, elementData);
            } else {
                return null;
            }
        }
    }
}
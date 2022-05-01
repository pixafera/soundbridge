using System;

namespace Pixa.Soundbridge {
    internal class MediaContainerMediaContainerCacheProvider : SoundbridgeListCacheProvider<MediaContainer> {
        private bool _listIsContainers;

        public MediaContainerMediaContainerCacheProvider(Soundbridge soundbridge, MediaContainer parent) : base(soundbridge, parent) {
        }

        protected override MediaContainer CreateObject(string elementData, int index) {
            if (index == 0) {
                var l = Soundbridge.Client.GetSongInfo(index);
                _listIsContainers = Array.IndexOf(l, "format: unsupported") >= 0;
            }

            if (_listIsContainers) {
                return new MediaContainer((MediaContainer)Parent, index, elementData);
            } else {
                return null;
            }
        }
    }
}
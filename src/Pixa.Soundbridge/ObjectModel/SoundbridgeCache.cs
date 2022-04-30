using System;
using System.Collections;
using System.Collections.Generic;

namespace Pixa.Soundbridge.Library
{
    /// <summary>
/// Caches List objects so that identical calls to ListXyz return the same
/// object.
/// </summary>
/// <remarks></remarks>
    public class SoundbridgeCache
    {
        private struct CacheKey
        {
            public object Parent;
            public Type ChildType;

            public CacheKey(SoundbridgeObject parent, Type childType)
            {
                Parent = parent;
                ChildType = childType;
            }

            public override int GetHashCode()
            {
                return Parent.GetHashCode() ^ ChildType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is CacheKey)
                {
                    CacheKey k = (CacheKey)obj;
                    return ReferenceEquals(k.ChildType, ChildType) & ReferenceEquals(k.Parent, Parent);
                }
                else
                {
                    return false;
                }
            }
        }

        private Dictionary<CacheKey, ISoundbridgeListCacheProvider> _cache = new Dictionary<CacheKey, ISoundbridgeListCacheProvider>();

        public SoundbridgeObjectCollection<T> BuildList<T>(SoundbridgeObject parent, string[] listData) where T : SoundbridgeListObject
        {
            var key = new CacheKey(parent, typeof(T));
            return (SoundbridgeObjectCollection<T>)_cache[key].BuildList(listData);
        }

        public void RegisterCache<T>(SoundbridgeListCacheProvider<T> cache) where T : SoundbridgeListObject
        {
            _cache.Add(new CacheKey(cache.Parent, typeof(T)), cache);
        }

        public void DeregisterCache(SoundbridgeObject parent, Type childType)
        {
            _cache.Remove(new CacheKey(parent, childType));
        }
    }

    public interface ISoundbridgeListCacheProvider
    {
        Type ChildType { get; }
        SoundbridgeObject Parent { get; }

        IList BuildList(string[] listData);
    }

    public abstract class SoundbridgeListCacheProvider<T> : ISoundbridgeListCacheProvider where T : SoundbridgeListObject
    {
        private Soundbridge _soundbridge;
        private SoundbridgeObject _parent;
        private SortedList<string, T> _cache = new SortedList<string, T>();

        public SoundbridgeListCacheProvider(Soundbridge soundbridge, SoundbridgeObject parent)
        {
            _parent = parent;
            _soundbridge = soundbridge;
        }

        public Type ChildType
        {
            get
            {
                return typeof(T);
            }
        }

        public SoundbridgeObject Parent
        {
            get
            {
                return _parent;
            }
        }

        public Soundbridge Soundbridge
        {
            get
            {
                return _soundbridge;
            }
        }

        public SoundbridgeObjectCollection<T> BuildList(string[] listData)
        {
            // Remove from the cache any elements not in listData
            string[] sortedListData = (string[])listData.Clone();
            Array.Sort(sortedListData);
            int listI = 0;
            int cacheI = 0;
            while (listI < sortedListData.Length & cacheI < _cache.Count)
            {
                string cache = _cache.Keys[cacheI];
                int comparison = string.Compare(_cache.Keys[cacheI], sortedListData[listI]);
                if (comparison >= 0)
                {
                    // Either the strings match, or there's a new element in listData
                    // So we need to move on to the next list item
                    listI += 1;
                }

                if (comparison < 0)
                {
                    // There's an element in the cache that's not in listdata, it needs
                    // to be removed, but we don't always want to remove cached objects
                    // i.e. A connected media server may not appear in ListServers if it's
                    // filtered out.
                    // If we don't remove the item, move onto the next element in the cache
                    if (!Remove(cacheI))
                    {
                        cacheI += 1;
                    }
                }
                else if (comparison == 0)
                {
                    // The strings match, so move on to the next element in the cache.
                    cacheI += 1;
                }
            }

            // Clear out the rest of the items in the cache, bearing in mind that we
            // don't always want to remove an element.
            while (cacheI < _cache.Count)
            {
                if (!Remove(cacheI))
                {
                    cacheI += 1;
                }
            }

            // Build the list, drawing existing elements from listData and creating new elements
            var l = CreateCollection();
            for (int i = 0, loopTo = listData.Length - 1; i <= loopTo; i++)
            {
                string s = listData[i];

                // Use the cache if we've seen the element before, otherwise create a new
                // object.  We may need to update the item's index as this is a new list.
                T o;
                if (_cache.ContainsKey(s))
                {
                    o = _cache[s];
                    o.Index = i;
                    l.Add(o);
                }
                else
                {
                    o = CreateObject(s, i);
                    if (o is object)
                    {
                        _cache.Add(s, o);
                        l.Add(o);
                    }
                }
            }

            return l;
        }

        private bool Remove(int index)
        {
            var o = _cache.Values[index];
            if (o.ShouldCacheDispose)
            {
                o.Dispose();
                _cache.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        private IList IBuildList(string[] listData)
        {
            return BuildList(listData);
        }

        IList ISoundbridgeListCacheProvider.BuildList(string[] listData) => IBuildList(listData);

        protected virtual SoundbridgeObjectCollection<T> CreateCollection()
        {
            return new SoundbridgeObjectCollection<T>(_soundbridge);
        }

        protected abstract T CreateObject(string elementData, int index);
    }

    internal class SoundbridgeMediaServerCacheProvider : SoundbridgeListCacheProvider<MediaServer>
    {
        public SoundbridgeMediaServerCacheProvider(Soundbridge soundbridge, SoundbridgeObject parent) : base(soundbridge, parent)
        {
        }

        protected override MediaServer CreateObject(string elementData, int index)
        {
            var tokens = elementData.Split(" ".ToCharArray(), 3, StringSplitOptions.None);
            return new MediaServer(Soundbridge, ServerListAvailabilityToMediaServerAvailability(tokens[0]), ServerListTypeToMediaServerType(tokens[1]), tokens[2], index);
        }

        /// <summary>
   /// Converts the specified string value into a <see cref="MediaServerAvailability"/>
   /// value.
   /// </summary>
        private MediaServerAvailability ServerListAvailabilityToMediaServerAvailability(string value)
        {
            switch (value ?? "")
            {
                case "kOnline":
                    {
                        return MediaServerAvailability.Online;
                    }

                case "kOffline":
                    {
                        return MediaServerAvailability.Offline;
                    }

                case "kHidden":
                    {
                        return MediaServerAvailability.Hidden;
                    }

                case "kInaccessible":
                    {
                        return MediaServerAvailability.Inaccessible;
                    }
            }

            return default;
        }

        /// <summary>
   /// Converts the specified value into a <see cref="MediaServerType"/>.
   /// </summary>
        private MediaServerType ServerListTypeToMediaServerType(string value)
        {
            switch (value ?? "")
            {
                case "kITunes":
                    {
                        return MediaServerType.Daap;
                    }

                case "kUPnP":
                    {
                        return MediaServerType.Upnp;
                    }

                case "kSlim":
                    {
                        return MediaServerType.Slim;
                    }

                case "kFlash":
                    {
                        return MediaServerType.Flash;
                    }

                case "kFavoriteRadio":
                    {
                        return MediaServerType.Radio;
                    }

                case "kAMTuner":
                    {
                        return MediaServerType.AM;
                    }

                case "kFMTuner":
                    {
                        return MediaServerType.FM;
                    }

                case "kRSP":
                    {
                        return MediaServerType.Rsp;
                    }

                case "kLinein":
                    {
                        return MediaServerType.LineIn;
                    }

                default:
                    {
                        return (MediaServerType)(-1);
                    }
            }
        }
    }

    internal class MediaContainerMediaContainerCacheProvider : SoundbridgeListCacheProvider<MediaContainer>
    {
        private bool _listIsContainers;

        public MediaContainerMediaContainerCacheProvider(Soundbridge soundbridge, MediaContainer parent) : base(soundbridge, parent)
        {
        }

        protected override MediaContainer CreateObject(string elementData, int index)
        {
            if (index == 0)
            {
                var l = Soundbridge.Client.GetSongInfo(index);
                _listIsContainers = Array.IndexOf(l, "format: unsupported") >= 0;
            }

            if (_listIsContainers)
            {
                return new MediaContainer((MediaContainer)Parent, index, elementData);
            }
            else
            {
                return null;
            }
        }
    }

    internal class MediaContainerSongCacheProvider : SoundbridgeListCacheProvider<Song>
    {
        private bool _listIsSongs;

        public MediaContainerSongCacheProvider(Soundbridge soundbridge, MediaContainer parent) : base(soundbridge, parent)
        {
        }

        protected override SoundbridgeObjectCollection<Song> CreateCollection()
        {
            return new SongCollection(Soundbridge);
        }

        protected override Song CreateObject(string elementData, int index)
        {
            if (index == 0)
            {
                var l = Soundbridge.Client.GetSongInfo(index);
                _listIsSongs = Array.IndexOf(l, "format: unsupported") < 0;
            }

            if (_listIsSongs)
            {
                return new Song((MediaContainer)Parent, index, elementData);
            }
            else
            {
                return null;
            }
        }
    }
}
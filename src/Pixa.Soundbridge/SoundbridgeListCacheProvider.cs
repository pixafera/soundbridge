using System;
using System.Collections;
using System.Collections.Generic;

namespace Pixa.Soundbridge
{
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
}
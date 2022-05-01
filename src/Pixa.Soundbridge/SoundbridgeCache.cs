using System;
using System.Collections.Generic;

namespace Pixa.Soundbridge
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
}
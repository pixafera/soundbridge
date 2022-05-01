using System;
using System.Collections.Generic;

namespace Pixa.Soundbridge
{
    public class SoundbridgeObjectCollection<T> : System.Collections.ObjectModel.Collection<T> where T : SoundbridgeListObject
    {
        private Soundbridge _sb;
        private Dictionary<string, T> _dict = new Dictionary<string, T>();

        public SoundbridgeObjectCollection(Soundbridge sb)
        {
            _sb = sb;
        }

        public T this[string key]
        {
            get
            {
                return _dict[key];
            }
        }

        public bool IsActive
        {
            get
            {
                return ReferenceEquals(Soundbridge.ActiveList, this);
            }
        }

        public Soundbridge Soundbridge
        {
            get
            {
                return _sb;
            }
        }

        public bool Contains(string key)
        {
            return _dict.ContainsKey(key);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _dict.Clear();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if (!_dict.ContainsKey(item.Name))
            {
                _dict.Add(item.Name, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            throw new NotSupportedException("Cannot remove items from a SoundbridgeObjectCollection");
        }

        protected override void SetItem(int index, T item)
        {
            throw new NotSupportedException("Cannot set items in SoundbridgeObjectCollections");
        }
    }
}
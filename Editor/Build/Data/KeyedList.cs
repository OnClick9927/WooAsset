using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WooAsset
{
    abstract class KeyedList<TKey, TItem>
    {
        [SerializeField] private List<TItem> values = new List<TItem>();
        private Dictionary<TKey, TItem> map = new Dictionary<TKey, TItem>();

        protected abstract TKey GetKey(TItem item);
        public void Add(TItem item)
        {
            var key = GetKey(item);
            if (map.ContainsKey(key))
            {
                throw new ArgumentException($"Item with key '{key}' already exists.");
            }
            values.Add(item);
            map.Add(key, item);
        }
        public bool Remove(TItem item)
        {
            if (values.Remove(item))
            {
                map.Remove(GetKey(item));
                return true;
            }
            return false;
        }
        public bool Remove(TKey key)
        {
            var find = Find(key);
            if (find != null)
                return Remove(key);
            return false;

        }

        public TItem Find(TKey key)
        {
            TItem item;
            if (!map.TryGetValue(key, out item))
            {
                item = values.FirstOrDefault(x => GetKey(x).Equals(key));
                if (item != null)
                    map.Add(key, item);
            }
            return item;

        }
        public bool ContainsKey(TKey key)
        {
            return Find(key) != null;
        }
        public List<TItem> GetValues() => values;

        public List<TItem> FindAll(Predicate<TItem> match)
        {
            List<TItem> list = new List<TItem>();
            for (int i = 0; i < values.Count; i++)
            {
                if (match(values[i]))
                {
                    list.Add(values[i]);
                }
            }
            return list;
        }
        public void SetList(List<TItem> items)
        {
            Clear();
            values.AddRange(items);
        }
        public void Clear()
        {
            values.Clear();
            map.Clear();
        }
    }
}

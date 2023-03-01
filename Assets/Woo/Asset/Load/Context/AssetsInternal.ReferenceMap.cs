

using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        public class ReferenceMap<T>
        {
            private Dictionary<T, int> map = new Dictionary<T, int>();
            public void Retain(T t)
            {
                if (!map.ContainsKey(t))
                {
                    map.Add(t, 0);
                }
                map[t] = map[t] + 1;
            }

            public int Release(T t)
            {
                if (!map.ContainsKey(t))
                {
                    map.Add(t, 0);
                }
                var count = map[t] - 1;
                if (count <= 0)
                    map.Remove(t);
                else
                    map[t] = count;
                return count;
            }
            public int GetCount(T t)
            {
                int count = 0;
                map.TryGetValue(t, out count);
                return count;
            }
        }
    }
}

using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private abstract class NameMap<T> where T : IAsset
        {
            private IAssetLife<T> listen;
            public void SetListen(IAssetLife<T> listen)
            {
                this.listen = listen;
            }



            private Dictionary<string, T> map = new Dictionary<string, T>();
            private List<T> values = new List<T>();
            public T Find(string name)
            {
                T result = default;
                map.TryGetValue(name, out result);
                return result;
            }
            protected abstract T CreateNew(string name, IAssetArgs args);

            protected T LoadAsync(string name, IAssetArgs args)
            {
                T result = Find(name);
                if (result == null)
                {
                    result = CreateNew(name, args);
                    map.Add(name, result);
                    values.Add(result);
                    result.LoadAsync();
                    listen?.OnAssetCreate(name, result);
                }
                result.Retain();

                listen?.OnAssetRetain(result, result.refCount);
                return result;

            }
            public abstract void Release(string name);
            protected int ReleaseRef(T t)
            {
                var count = t.Release();
                listen?.OnAssetRelease(t, count);
                return count;
            }

            protected List<string> GetZeroRefKeys(List<string> result)
            {
                result.Clear();
                foreach (var item in map)
                    if (item.Value.refCount == 0) result.Add(item.Key);
                return result;
            }
            protected void TryRealUnload(string path)
            {
                T asset = Find(path);
                if (asset.refCount != 0) return;
                asset.UnLoad();
                Remove(path);
            }
            protected void Remove(string path)
            {
                T asset = Find(path);
                map.Remove(path);
                values.Remove(asset);
                listen?.OnAssetUnload(path, asset);
            }
            public IReadOnlyList<T> GetAll()
            {
                return values;
            }
        }
    }
}

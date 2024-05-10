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
            public T Find(string uid)
            {
                T result = default;
                map.TryGetValue(uid, out result);
                return result;
            }
            protected abstract T CreateNew(IAssetArgs args);

            public void RetainRef(T asset)
            {
                if (asset == null) return;
                asset.Retain();
                listen?.OnAssetRetain(asset, asset.refCount);
            }
            public void RetainRef(string uid) => RetainRef(Find(uid));


            public T LoadAsync(IAssetArgs args)
            {
                string uid = args.uid;
                T result = Find(uid);
                if (result == null)
                {
                    result = CreateNew(args);
                    map.Add(uid, result);
                    values.Add(result);
                    result.LoadAsync();
                    listen?.OnAssetCreate(uid, result);
                }
                OnRetain(result, result.refCount != 0);
                return result;
            }
            protected abstract void OnRetain(T asset, bool old);
            public abstract void Release(string uid);
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
            protected void TryRealUnload(string uid)
            {
                T asset = Find(uid);
                if (asset.refCount != 0) return;
                asset.UnLoad();
                Remove(uid);
            }
            protected void Remove(string uid)
            {
                T asset = Find(uid);
                map.Remove(uid);
                values.Remove(asset);
                listen?.OnAssetUnload(uid, asset);
            }
            public IReadOnlyList<T> GetAll()
            {
                return values;
            }
        }
    }
}

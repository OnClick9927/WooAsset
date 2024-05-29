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
            protected int ReleaseRef(T t)
            {
                var count = t.Release();
                listen?.OnAssetRelease(t, count);
                return count;
            }





            public T LoadAsync(IAssetArgs args)
            {
                string uid = args.uid;
                T result = Find(uid);
                if (result == null)
                {
                    result = CreateNew(args);
                    map.Add(uid, result);
                    result.LoadAsync();
                    listen?.OnAssetCreate(uid, result);
                }
                OnRetain(result, result.refCount != 0);
                return result;
            }
            protected virtual void OnRetain(T asset, bool old)
            {
                RetainRef(asset);
            }




            public abstract void Release(string uid);

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
                map.Remove(uid);
                listen?.OnAssetUnload(uid, asset);
            }






            public int GetCount() => map.Count;
            public IEnumerable<string> GetKeys() => map.Keys;
        }
    }
}

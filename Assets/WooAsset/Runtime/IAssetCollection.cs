using System.Collections.Generic;

namespace WooAsset
{
    public interface IAssetCollection
    {
        bool Add(AssetHandle handle);
        AssetHandle Get(string path, System.Func<AssetHandle> create);
        AssetHandle Find(string path);
        void Clear();
    }
    class AssetCollection : IAssetCollection
    {
        static Queue<AssetCollection> collections = new Queue<AssetCollection>();
        private AssetCollection() { }
        public static AssetCollection Get()
        {
            if (collections.Count == 0)
                return new AssetCollection();
            return collections.Dequeue();
        }
        public static void Set(AssetCollection collection)
        {
            if (collection.map.Count != 0)
            {
                AssetsHelper.LogError("AssetCollection is Not Empty");
                return;
            }
            collections.Enqueue(collection);
        }

        private Dictionary<string, AssetHandle> map = new Dictionary<string, AssetHandle>();
        public bool Add(AssetHandle handle)
        {
            AssetHandle _h;
            if (map.TryGetValue(handle.path, out _h))
            {
                AssetsHelper.LogError($"ready exist handle with path-->{handle.path}");
                return false;
            }
            map.Add(handle.path, handle);
            return true;
        }
        public AssetHandle Get(string path, System.Func<AssetHandle> create)
        {
            AssetHandle handle = Find(path);
            if (handle == null)
            {
                handle = create?.Invoke();
                Add(handle);
            }
            return handle;
        }
        public AssetHandle Find(string path)
        {
            AssetHandle handle = null;
            map.TryGetValue(path, out handle);
            return handle;
        }
        public void Clear()
        {
            foreach (AssetHandle handle in map.Values)
                Assets.Release(handle);
            map.Clear();
        }
    }

}

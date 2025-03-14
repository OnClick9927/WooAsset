using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    interface IAssetBridge
    {
        void Release();
        bool CouldRelease();
    }
    public class ObjectBridge<T> : AssetBridge<T> where T : Object
    {
        public ObjectBridge(T context, AssetHandle handle) : base(context, handle) { }

        protected override bool CouldRelease()
        {
            return context == null;
        }
    }
    public class GameObjectBridge : ObjectBridge<GameObject>
    {
        public GameObjectBridge(GameObject context, AssetHandle handle) : base(context, handle)
        {
        }
    }
    public abstract class AssetBridge<T> : IAssetBridge where T : class
    {
        private AssetHandle handle;

        public T context;
        protected AssetBridge(T context, AssetHandle handle)
        {
            this.context = context;
            this.handle = handle;
        }
        protected abstract bool CouldRelease();

        bool IAssetBridge.CouldRelease()
        {
            return CouldRelease();
        }
        void IAssetBridge.Release()
        {
            Assets.Release(handle);
        }
    }

    public class AssetCollection
    {
        private Dictionary<string, AssetHandle> map = new Dictionary<string, AssetHandle>();
        private void Add(AssetHandle handle)
        {
            map.Add(handle.path, handle);
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

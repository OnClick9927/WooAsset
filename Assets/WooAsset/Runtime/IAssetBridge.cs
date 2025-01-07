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
    public abstract class AssetBridge : IAssetBridge
    {
        private AssetHandle handle;
        protected AssetBridge(AssetHandle handle)
        {
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
    public abstract class AssetBridge<T> : AssetBridge where T : class
    {
        public T context;
        protected AssetBridge(T context, AssetHandle handle) : base(handle)
        {
            this.context = context;
        }
    }
}

using UnityEngine;

namespace WooAsset
{
    public class ObjectBridge<T> : AssetBridge<T> where T : Object
    {
        public ObjectBridge(T context, AssetHandle handle) : base(context, handle) { }

        protected override bool CouldRelease()
        {
            return context;
        }
    }
}

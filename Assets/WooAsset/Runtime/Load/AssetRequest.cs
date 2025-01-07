using UnityEngine;

namespace WooAsset
{
    public abstract class AssetRequest : Operation
    {
        public abstract UnityEngine.Object asset { get; }
        public abstract UnityEngine.Object[] allAssets { get; }
    }
    class RuntimeAssetRequest : AssetRequest
    {
        private AssetBundleRequest request;

        public RuntimeAssetRequest(AssetBundleRequest request)
        {
            this.request = request;
            Done();
        }
        private async void Done()
        {
            await request;
            InvokeComplete();
        }
        public override float progress => request == null ? 0 : request.progress;
        public override UnityEngine.Object asset => !isDone ? null : request.asset;
        public override UnityEngine.Object[] allAssets => !isDone ? null : request.allAssets;
    }
}

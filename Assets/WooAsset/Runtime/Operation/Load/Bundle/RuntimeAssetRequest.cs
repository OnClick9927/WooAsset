using UnityEngine;

namespace WooAsset
{
    public class RuntimeAssetRequest : AssetRequest
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

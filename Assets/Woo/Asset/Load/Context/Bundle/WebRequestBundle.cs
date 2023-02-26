using UnityEngine;
using static WooAsset.AssetsInternal;

namespace WooAsset
{
    public class WebRequestBundle : Bundle
    {
        public WebRequestBundle(BundleLoadArgs loadArgs) : base(loadArgs)
        {
        }
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (loadOp == null) return downloader.progress * 0.5f;
                return downloader.progress * 0.5f + loadOp.progress * 0.5f;
            }
        }

        private Downloader downloader;
        private AssetBundleCreateRequest loadOp;

        protected async override void OnLoad()
        {
            downloader = new Downloader(loadArgs.path);
            await downloader.Start();
            if (!downloader.isError)
            {
                byte[] buffer = downloader.data;

                buffer = AssetEncryptStream.DeCode(loadArgs.bundleName, buffer);
                loadOp = await AssetBundle.LoadFromMemoryAsync(buffer);
                SetResult(loadOp.assetBundle);
            }
            else
            {
                SetResult(null);
            }
        }
        protected override void OnUnLoad()
        {
            value.Unload(true);
            Resources.UnloadUnusedAssets();
        }
    }
}

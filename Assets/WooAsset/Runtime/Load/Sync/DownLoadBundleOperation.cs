namespace WooAsset
{
    public class DownLoadBundleOperation : AssetOperation
    {
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return downloader.progress * 0.9f;
            }
        }
        public string bundleName { get; private set; }
        private BundleDownloader downloader;

        public DownLoadBundleOperation(string bundleName)
        {
            this.bundleName = bundleName;
            Done();
        }
        private async void Done()
        {
            downloader = AssetsInternal.DownLoadBundle(bundleName);
            await downloader;
            if (downloader.isErr)
            {
                SetErr(downloader.error);
            }
            else
            {
                await downloader.SaveBundleToLocal();
            }

            InvokeComplete();
        }

    }

}

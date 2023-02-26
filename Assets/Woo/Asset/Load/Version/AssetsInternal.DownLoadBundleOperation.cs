


namespace WooAsset
{
    partial class AssetsInternal
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
            private Downloader downloader;

            public DownLoadBundleOperation(string bundleName)
            {
                this.bundleName = bundleName;
                Done();
            }
            private async void Done()
            {
                string url = AssetsInternal.GetUrlFromBundleName(bundleName);
                string writePath = AssetsInternal.GetBundleLocalPath(bundleName);
                downloader = new Downloader(url, writePath);
                await downloader.Start();
                if (downloader.isError)
                {
                    SetErr(downloader.error);
                }
                InvokeComplete();
            }

        }
    }
}

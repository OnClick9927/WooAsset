namespace WooAsset
{
    public class BundleDownloader : Downloader
    {
        protected string bundleName;
        public BundleDownloader(string url, string bundleName) : base(url, AssetsInternal.GetWebRequestTimeout(), AssetsInternal.GetWebRequestRetryCount())
        {
            this.bundleName = bundleName;
        }

        public virtual Operation SaveBundleToLocal()
        {
            string path = AssetsInternal.GetBundleLocalPath(bundleName);
            return AssetsHelper.WriteFile(
                            this.data,
                            path,
                            true
                            );
        }
    }


}

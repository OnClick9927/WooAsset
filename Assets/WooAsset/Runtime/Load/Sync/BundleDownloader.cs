using System.IO;

namespace WooAsset
{
    public class BundleDownloader : Downloader
    {
        protected string bundleName;
        public BundleDownloader(string url, string bundleName) : base(url, AssetsInternal.GetWebRequestTimeout(), AssetsInternal.GetWebRequestRetryCount())
        {
            this.bundleName = bundleName;
        }

        public virtual void SaveBundleToLocal()
        {
            string path = AssetsInternal.GetBundleLocalPath(bundleName);
            File.WriteAllBytes(path, this.data);
        }
    }


}

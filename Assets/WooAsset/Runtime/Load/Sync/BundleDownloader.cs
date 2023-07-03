using System.IO;

namespace WooAsset
{
    public class BundleDownloader : Downloader
    {
        protected string bundleName;
        public BundleDownloader(string url, float timeout,string bundleName) 
        {
            this.timeout = timeout;
            this.url = url;
            this.bundleName = bundleName;
            Start();
        }

        public virtual void SaveBundleToLocal()
        {
            string path = AssetsInternal.GetBundleLocalPath(bundleName);
            File.WriteAllBytes(path, this.data);
        }
    }


}

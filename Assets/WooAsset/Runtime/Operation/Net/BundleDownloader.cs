using UnityEngine;
using UnityEngine.Networking;

namespace WooAsset
{
    public class BundleDownloader : Downloader
    {
        public BundleDownloader(string url, int timeout, int retry) : base(url, timeout, retry) { }


        public AssetBundle bundle { get; private set; }

        protected override UnityWebRequest GetRequset()
        {
            return new UnityWebRequest(url, "Get", new DownloadHandlerAssetBundle(url, 0), null);
        }
        protected override void OnRequestEnd(UnityWebRequest req)
        {
            this.bundle = (req.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
        }
    }

}


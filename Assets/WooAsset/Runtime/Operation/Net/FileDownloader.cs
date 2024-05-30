using UnityEngine.Networking;

namespace WooAsset
{
    public class FileDownloader : Downloader
    {
        protected string path;
        public FileDownloader(string url, string path, int timeout, int retry) : base(url, timeout, retry)
        {
            this.path = path;
        }
        protected override UnityWebRequest GetRequset()
        {
            return new UnityWebRequest(url, "Get", new DownloadHandlerFile(path), null);
        }
        protected override void OnRequestEnd(UnityWebRequest req)
        {
        }


    }

}


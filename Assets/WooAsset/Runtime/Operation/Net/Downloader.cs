

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace WooAsset
{
    public class Downloader : LoopOperation
    {
        public string url { get; protected set; }
        public int timeout { get; protected set; }
        public byte[] data { get; protected set; }

        public override float progress { get { return _progress; } }
        private float _progress;
        protected int retry;
        private int _retry = 0;
        private UnityWebRequest request;
        protected Downloader() { }
        public Downloader(string url, int timeout, int retry)
        {
            this.timeout = timeout;
            this.retry = retry;
            this.url = url;
        }
        protected virtual UnityWebRequest GetRequset()
        {

            return UnityWebRequest.Get(url);

        }
        protected virtual void OnRequestEnd(UnityWebRequest req)
        {
            var data = req.downloadHandler.data;
            this.data = new byte[data.Length];
            Array.Copy(data, this.data, data.Length);
        }

        protected override void OnUpdate()
        {
            if (request == null)
            {
                request = GetRequset();
                request.timeout = timeout;
                var _ = request.SendWebRequest();
            }
            if (request.isDone)
            {
                bool success = true;
#if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (req.isHttpError || req.isNetworkError)
#endif
                {
                    success = false;
                  
                }


                if (!success)
                {
                    if (_retry < retry)
                    {
                        _retry++;
                    }
                    else
                    {
                        SetErr($"{request.error}:{url}");
                        InvokeComplete();
                    }
                    request.Dispose(); 
                    request = null;
                }
                else
                {
                    OnRequestEnd(request);
                    request.Dispose();
                    InvokeComplete();
                }
            }
        }
    }
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


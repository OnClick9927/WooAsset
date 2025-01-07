

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace WooAsset
{
    public class DownLoader : LoopOperation
    {
        public string url { get; protected set; }
        public int timeout { get; protected set; }
        public byte[] data { get; protected set; }

        public override float progress { get { return _progress; } }
        private float _progress;
        protected int retry;
        private int _retry = 0;
        private UnityWebRequest request;
        protected DownLoader() { }
        public DownLoader(string url, int timeout, int retry)
        {
            this.timeout = timeout;
            this.retry = retry;
            this.url = url;
        }
        protected virtual UnityWebRequest GetRequest()
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
                request = GetRequest();
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
            else
            {
                _progress = request.downloadProgress;
            }
        }
    }
    public class FileDownLoader : DownLoader
    {
        protected string path;
        public FileDownLoader(string url, string path, int timeout, int retry) : base(url, timeout, retry)
        {
            this.path = path;
        }
        protected override UnityWebRequest GetRequest()
        {
            return new UnityWebRequest(url, "Get", new DownloadHandlerFile(path), null);
        }
        protected override void OnRequestEnd(UnityWebRequest req)
        {
        }


    }
    public class BundleDownLoader : DownLoader
    {
        private Hash128 hash;
        private uint crc;
        private bool cache;
        public BundleDownLoader(string url, bool cache, uint crc, Hash128 hash, int timeout, int retry) : base(url, timeout, retry)
        {
            this.hash = hash;
            this.crc = crc;
            this.cache = cache;
        }


        public AssetBundle bundle { get; private set; }

        protected override UnityWebRequest GetRequest()
        {
            DownloadHandlerAssetBundle download;
            if (cache)
                download = new DownloadHandlerAssetBundle(url, hash, crc);
            else
                download = new DownloadHandlerAssetBundle(url, 0);

#if UNITY_2020_3_OR_NEWER
            download.autoLoadAssetBundle = false;
#endif


            return new UnityWebRequest(url, "Get", download, null); ;
        }
        protected override void OnRequestEnd(UnityWebRequest req)
        {

            this.bundle = (req.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
        }
    }
}


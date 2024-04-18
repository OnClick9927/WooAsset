using System;
using UnityEngine;
using UnityEngine.Networking;

namespace WooAsset
{
    public class BundleDownloader : Downloader
    {
        protected string bundleName;
        public BundleDownloader(string url, string bundleName, int retry, int timeout) : base(url, timeout, retry)
        {
            this.bundleName = bundleName;
        }

        public virtual Operation SaveBundleToLocal()
        {
            if (!AssetsInternal.GetSaveBundlesWhenPlaying()) return new EmptyOperation();

            string path = AssetsInternal.GetBundleLocalPath(bundleName);
            return AssetsHelper.WriteFile(
                            this.data,
                            path,
                            true
                            );
        }

    }
    public class BundleDownloader2 : Operation
    {
        public override float progress => _progress;
        protected string bundleName;
        private int _retry;
        private float _progress;
        private readonly int retry;
        private string url;
        private int timeout;
        private DownloadHandlerAssetBundle _downloadhandler;
        public AssetBundle bundle { get; private set; }

        public BundleDownloader2(string url, string bundleName, int retry, int timeout)
        {
            this.url = url;
            this.bundleName = bundleName;

            Start();
            _retry = retry;
            this.timeout = timeout;
        }
        protected virtual async void Start()
        {
            AssetsHelper.Log($"Download start: {url}");
            _downloadhandler = new DownloadHandlerAssetBundle(url, 0);

        DownLoad:
            UnityWebRequest req = new UnityWebRequest();
            req.downloadHandler = _downloadhandler;
            req.timeout = timeout;
            var _ = req.SendWebRequest();
            while (!req.isDone)
            {
                _progress = req.downloadProgress;
                await new YieldOperation();
            }
#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
            if (req.isHttpError || req.isNetworkError)
#endif
            {
                if (_retry < retry)
                {
                    _retry++;
                    req.Dispose();
                    goto DownLoad;
                }
                else
                {
                    SetErr($"{req.error}:{url}");
                }
            }
            else
            {
                if (!isErr)
                {
                    this.bundle = _downloadhandler.assetBundle;
                }
            }
            req.Dispose();
            InvokeComplete();
        }
    }


}

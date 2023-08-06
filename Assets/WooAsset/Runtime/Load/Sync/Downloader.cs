

using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace WooAsset
{
    public class Downloader : AssetOperation
    {
        public string url { get; protected set; }
        public int timeout { get; protected set; }
        public byte[] data { get; protected set; }

        public override float progress { get { return _progress; } }
        private float _progress;
        private readonly int retry;
        private int _retry = 0;
        protected Downloader() { }
        public Downloader(string url, int timeout, int retry)
        {
            this.timeout = timeout;
            this.retry = retry;
            this.url = url;
            Start();
        }
        protected virtual async void Start()
        {
            AssetsInternal.Log($"Download start: {url}");
        DownLoad:
            var req = UnityWebRequest.Get(url);
            req.timeout = timeout;
            var _ = req.SendWebRequest();
            while (!req.isDone)
            {
                _progress = req.downloadProgress;
                await Task.Yield();
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
                    var data = req.downloadHandler.data;
                    this.data = new byte[data.Length];
                    Array.Copy(data, this.data, data.Length);
                }
            }
            req.Dispose();
            InvokeComplete();
        }
    }


}

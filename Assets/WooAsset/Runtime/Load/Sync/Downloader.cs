

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace WooAsset
{
    public class Downloader : AssetOperation
    {
        public string url { get; protected set; }
        public float timeout { get; protected set; }
        public byte[] data { get; protected set; }

        public override float progress { get { return _progress; } }
        private float _progress;
        protected Downloader() { }
        public Downloader(string url, float timeout)
        {
            this.timeout = timeout;
            this.url = url;
            Start();
        }

        protected virtual async void Start()
        {
            AssetsInternal.Log($"Download start: {url}");
            var req = UnityWebRequest.Get(url);
            var _ = req.SendWebRequest();
            ulong lastDownloaded = 0;
            var lastTime = Time.realtimeSinceStartup;
            while (!req.isDone)
            {
                var time = Time.realtimeSinceStartup;
                var downloaded = req.downloadedBytes;
                if (lastDownloaded == downloaded)
                {
                    if (time - lastTime >= timeout)
                    {
                        req.Abort();
                        SetErr($"timeout:{url}");
                        break;
                    }
                }
                else
                {
                    _progress = req.downloadProgress;
                    lastTime = time;
                    lastDownloaded = downloaded;
                }
                await Task.Yield();
            }
#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
                if (req.isHttpError || req.isNetworkError)
#endif
            {
                SetErr($"{req.error}:{url}");
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

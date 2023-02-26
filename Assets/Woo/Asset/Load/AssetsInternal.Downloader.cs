

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace WooAsset
{
    partial class AssetsInternal
    {
        public class Downloader : AssetOperation
        {
            public bool isError { get; private set; }
            public string url { get; private set; }
            public string path { get; private set; }
            public float timeout { get; private set; }
            public int retries { get; private set; }
            public byte[] data { get; private set; }
            public string text { get { return data == null ? null : System.Text.Encoding.UTF8.GetString(data); } }
            public override float progress { get { return _progress; } }
            private float _progress;
            private float _lastRetries;

            public Downloader(string url, string downloadPath = null, float timeout = 0, int retries = 3)
            {
                if (timeout == 0) timeout = GetWebRequestTimeout();
                this.url = url;
                this.path = downloadPath;
                this.timeout = timeout;
                this.retries = retries;
            }

            public async Task Start()
            {
                //TODO: Remove debug log
                Debug.Log($"Download start: {url}, {path}");
                //TODO: 断点续传，暂时感觉不需要
                var req = UnityWebRequest.Get(url);
                var _ = req.SendWebRequest();
                ulong lastDownloaded = 0;
                var lastTime = GetTime();
                while (!req.isDone)
                {
                    var time = GetTime();
                    var downloaded = req.downloadedBytes;
                    if (lastDownloaded == downloaded)
                    {
                        if (time - lastTime >= timeout)
                        {
                            req.Abort();
                            Timeout();
                            return;
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
                    req.Dispose();
                    Failed(req.error);
                }
                else
                {
                    var data = req.downloadHandler.data;
                    this.data = new byte[data.Length];
                    Array.Copy(data, this.data, data.Length);
                    if (!string.IsNullOrEmpty(path))
                    {
                        File.WriteAllBytes(path, this.data);
                    }
                    req.Dispose();
                    Done();
                }
            }

            private float GetTime()
            {
                return Time.realtimeSinceStartup;
            }

            private void Timeout()
            {
                if (_lastRetries > 0) Retry();
                else Failed("Failed cause timeout.");
            }

            private async void Retry()
            {
                _lastRetries -= 1;
                _progress = 0;
                await Start();
            }

            private void Failed(string err)
            {
                this.isError = true;
                SetErr(err);
                InvokeComplete();
            }

            private void Done()
            {
                this.isError = !isNormalMode;
                InvokeComplete();
            }
        }
    }
}

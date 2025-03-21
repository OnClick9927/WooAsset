

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace WooAsset
{
    public abstract class DownLoader : LoopOperation
    {
        private class DownLoaderQueue
        {
            private Queue<DownLoader> queue;
            private int count = 0;
            public void Run(DownLoader downLoader)
            {
                if (count < RequestCountAtSameTime)
                {
                    count++;
                    downLoader._state = State.Run;
                }
                else
                {
                    if (queue == null)
                        queue = new Queue<DownLoader>();
                    queue.Enqueue(downLoader);
                    downLoader._state = State.Wait;
                }
            }
            public void DownLoadEnd(DownLoader downLoader)
            {
                count--;
                if (queue == null || queue.Count == 0) return;
                var dequeue = queue.Dequeue();
                Run(dequeue);
            }

            public void Clear()
            {
                count = 0;
            }
        }


        static DownLoaderQueue _queue;
        static DownLoaderQueue queue
        {
            get
            {
                if (_queue == null)
                    _queue = new DownLoaderQueue();
                return _queue;
            }
        }
        public static void ClearQueue() => queue.Clear();
        public string url { get; protected set; }
        public int timeout { get; protected set; }

        public override float progress { get { return _progress; } }
        private float _progress;
        protected int retry;
        private int _retry = 0;
        private UnityWebRequest request;
        public static int RequestCountAtSameTime;
        private enum State
        {
            Wait, Run
        }
        private State _state;
        protected DownLoader() { }
        public DownLoader(string url, int timeout, int retry)
        {
            this.timeout = timeout;
            this.retry = retry;
            this.url = url;
        }
        protected override void AddToLoop()
        {
            base.AddToLoop();
            queue.Run(this);
        }
        protected abstract UnityWebRequest GetRequest();
        protected abstract void OnRequestEnd(UnityWebRequest request);

        protected override void OnUpdate()
        {
            if (_state == State.Wait) return;
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
                    queue.DownLoadEnd(this);
                }
            }
            else
            {
                _progress = request.downloadProgress;
            }
        }


    }

    public class BytesDownLoader : DownLoader
    {
        public byte[] data { get; protected set; }
        public BytesDownLoader(string url, int timeout, int retry) : base(url, timeout, retry)
        {

        }
        protected override UnityWebRequest GetRequest() => UnityWebRequest.Get(url);
        protected override void OnRequestEnd(UnityWebRequest request)
        {
            var data = request.downloadHandler.data;
            this.data = new byte[data.Length];
            Array.Copy(data, this.data, data.Length);
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
        protected override void OnRequestEnd(UnityWebRequest request)
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
        protected override void OnRequestEnd(UnityWebRequest request) => this.bundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
    }
}


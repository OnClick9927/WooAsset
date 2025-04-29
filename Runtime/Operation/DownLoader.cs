

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace WooAsset
{

    class DownLoaderSystem
    {
        private static Dictionary<string, List<DownLoader>> memory = new Dictionary<string, List<DownLoader>>();
        private static List<DownLoader> update = new List<DownLoader>();


        private static void DownLoad(DownLoader down)
        {
            update.Add(down);
            Run(down);
            var is_busy = AssetsLoop.isBusy;
        }




        public static FileDownLoader File(string url, string path, int timeout, int retry)
        {
            var fd = new FileDownLoader(url, path, timeout, retry);
            DownLoad(fd);
            return fd;
        }
        private static void ReCycleDownLoader(DownLoader loader)
        {
        }

        public static BytesDownLoader Bytes(string url, int timeout, int retry)
        {
            var list = AssetsHelper.GetFromDictionary(memory, url);
        Again:
            if (list == null || list.Count == 0)
            {
                var fd = new BytesDownLoader(url, timeout, retry);
                list.Add(fd);
                DownLoad(fd);
                return fd;
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var downLoader = list[i];
                if (downLoader.isErr)
                {
                    list.RemoveAt(i);
                    ReCycleDownLoader(downLoader);
                    list = null;
                    continue;
                }
                if (downLoader is BytesDownLoader)
                {
                    var bd = downLoader as BytesDownLoader;
                    return bd;
                }
            }
            list = null;
            goto Again;
        }

        public static BundleDownLoader Bundle(string url, bool cache, uint crc, Hash128 hash, int timeout, int retry)
        {
            var list = AssetsHelper.GetFromDictionary(memory, url);
        Again:
            if (list == null || list.Count == 0)
            {
                var fd = new BundleDownLoader(url, cache, crc, hash, timeout, retry);
                list.Add(fd);
                DownLoad(fd);
                return fd;
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var downLoader = list[i];
                if (downLoader.isErr)
                {
                    list.RemoveAt(i);
                    ReCycleDownLoader(downLoader);

                    list = null;
                    continue;
                }
                if (downLoader is BundleDownLoader)
                {
                    var bd = downLoader as BundleDownLoader;
                    if (bd.cache == cache && bd.crc == crc && bd.hash == hash)
                    {
                        return bd;
                    }
                }
            }
            list = null;
            goto Again;
        }


        public static void Update()
        {
            for (var i = update.Count - 1; i >= 0; i--)
            {
                var context = update[i];
                if (context.isDone)
                    update.RemoveAt(i);
                context.Update();
                if (context.isDone)
                    update.RemoveAt(i);
            }

        }



        private static Queue<DownLoader> queue = new Queue<DownLoader>();
        private static int count = 0;
        private static void Run(DownLoader downLoader)
        {
            if (count < DownLoader.RequestCountAtSameTime)
            {
                count++;
                downLoader._state = DownLoaderState.Run;
            }
            else
            {
                if (queue == null)
                    queue = new Queue<DownLoader>();
                queue.Enqueue(downLoader);
                downLoader._state = DownLoaderState.Wait;
            }
        }
        internal static void DownLoadEnd(DownLoader downLoader)
        {
            count--;
            if (queue == null || queue.Count == 0) return;
            var dequeue = queue.Dequeue();
            Run(dequeue);
        }
        internal static void ClearQueue()
        {
            count = 0;
            queue.Clear();
            memory.Clear();
            update.Clear();
        }
    }
    internal enum DownLoaderState
    {
        Wait, Run
    }

    public abstract class DownLoader : Operation
    {
        public string url { get; protected set; }
        public int timeout { get; protected set; }

        public override float progress { get { return _progress; } }
        private float _progress;
        protected int retry;
        private int _retry = 0;
        private UnityWebRequest request;
        public static int RequestCountAtSameTime;

        internal DownLoaderState _state;

        public DownLoader(string url, int timeout, int retry)
        {
            this.timeout = timeout;
            this.retry = retry;
            this.url = url;
        }

        protected abstract UnityWebRequest GetRequest();
        protected abstract void OnRequestEnd(UnityWebRequest request);

        internal void Update()
        {
            if (_state == DownLoaderState.Wait) return;
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
                    DownLoaderSystem.DownLoadEnd(this);
                }
            }
            else
            {
                _progress = request.downloadProgress;
            }
        }

        public static void ClearQueue()
        {
            DownLoaderSystem.ClearQueue();
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
            this.data = request.downloadHandler.data;
            //var data = request.downloadHandler.data;
            //this.data = new byte[data.Length];

            //Array.Copy(data, this.data, data.Length);
        }

    }
    public class FileDownLoader : DownLoader
    {
        public string path;
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
        public Hash128 hash;
        public uint crc;
        public bool cache;
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


using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{

    abstract class AssetRequest : Operation
    {
        public abstract UnityEngine.Object asset { get; }
        public abstract UnityEngine.Object[] allAssets { get; }

    }
    class RuntimeAssetRequest : AssetRequest
    {
        private AssetBundleRequest request;
        private static Queue<RuntimeAssetRequest> cache = new Queue<RuntimeAssetRequest>();


        public static AssetRequest Request(AssetBundleRequest request)
        {
            RuntimeAssetRequest req = null;
            if (cache.Count == 0)
                req = new RuntimeAssetRequest();
            else
            {
                req = cache.Dequeue();
                req.ResetIsDone();
            }
            req.request = request;
            req.Done();
            return req;
        }

        //public RuntimeAssetRequest(AssetBundleRequest request)
        //{
        //    this.request = request;
        //    Done();
        //}
        private async void Done()
        {
            //ResetIsDone();
            await request;
            InvokeComplete();
            request = null;
            cache.Enqueue(this);
        }
        public override float progress => request == null ? 0 : request.progress;
        public override UnityEngine.Object asset => !isDone ? null : request.asset;
        public override UnityEngine.Object[] allAssets => !isDone ? null : request.allAssets;
    }
}

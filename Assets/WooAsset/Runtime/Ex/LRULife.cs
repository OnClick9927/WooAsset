

using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    public class LRULife : IAssetLife<Bundle>
    {
        private long maxLength;
        private long length;
        private Dictionary<string, Bundle> map;
        private LinkedList<string> list;
        private bool work;
        private Queue<Bundle> queue = new Queue<Bundle>();
        private void ReleaseBundle(Bundle bundle)
        {
            string bundleName = bundle.bundleName;
            IReadOnlyList<string> assets = Assets.GetAllAssetPaths(bundleName);
            AssetsHelper.LogWarning($"Unload Bundle {bundleName} Cos Of Memory Size \n{string.Join(",", assets)}");
            if (assets != null)
            {
                for (int i = 0; i < assets.Count; i++)
                {
                    string assetPath = assets[i];
                    if (!Assets.GetIsAssetLoaded(assetPath)) continue;
                    Assets.Release(assetPath);
                }
            }
        }


        private async void _Put()
        {
            work = true;
            while (queue.Count > 0)
            {
                if (!Application.isPlaying) break;
                Bundle bundle = queue.Dequeue();
                string bundleName = bundle.bundleName;
                if (!map.ContainsKey(bundleName))
                {
                    map.Add(bundleName, bundle);
                    await bundle;
                    length += bundle.assetLength;
                }
                else
                {
                    list.Remove(bundleName);
                }
                list.AddLast(bundleName);

                if (!Application.isPlaying) break;
                if (length > maxLength)
                {
                    string _bundleName = list.First.Value;
                    Bundle _bundle = map[_bundleName];
                    ReleaseBundle(_bundle);
                    length -= _bundle.assetLength;
                    map.Remove(_bundleName);
                    list.RemoveFirst();
                }
            }
            work = false;
        }
        private void Put(Bundle bundle)
        {
            if (!Application.isPlaying) return;
            queue.Enqueue(bundle);
            if (work) return;
            _Put();
        }


        public LRULife(long maxLength)
        {
            this.maxLength = maxLength;
            list = new LinkedList<string>();
            map = new Dictionary<string, Bundle>();
        }


        void IAssetLife<Bundle>.OnAssetCreate(string path, Bundle asset) => Put(asset);
        void IAssetLife<Bundle>.OnAssetRetain(Bundle asset, int count) => Put(asset);

        void IAssetLife<Bundle>.OnAssetRelease(Bundle asset, int count) { }


        void IAssetLife<Bundle>.OnAssetUnload(string path, Bundle asset) {
        }
    }
}

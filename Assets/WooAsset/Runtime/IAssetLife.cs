

using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    public interface IAssetLife { }
    public interface IAssetLife<T>: IAssetLife where T : AssetOperation
    {
        void OnAssetCreate(string path, T asset);
        void OnAssetRetain(T asset, int count);
        void OnAssetRelease(T asset, int count);
        void OnAssetUnload(string path, T asset);
    }
    class MixedAssetLife : IAssetLife<AssetHandle>, IAssetLife<Bundle>
    {

        private List<IAssetLife<AssetHandle>> assets = new List<IAssetLife<AssetHandle>>();
        private List<IAssetLife<Bundle>> bundles = new List<IAssetLife<Bundle>>();


        public void AddLife(IAssetLife life)
        {
            if (life == null) return;
            if (life is IAssetLife<AssetHandle>)
                assets.Add(life as IAssetLife<AssetHandle>);
            if (life is IAssetLife<Bundle>)
                bundles.Add(life as IAssetLife<Bundle>);
        }

        public void RemoveAssetLife(IAssetLife life)
        {
            if (life == null) return;
            if (life is IAssetLife<AssetHandle>)
                assets.Remove(life as IAssetLife<AssetHandle>);
            if (life is IAssetLife<Bundle>)
                bundles.Remove(life as IAssetLife<Bundle>);
        }



        void IAssetLife<AssetHandle>.OnAssetCreate(string path, AssetHandle asset)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetCreate(path, asset);
            }
        }

        void IAssetLife<Bundle>.OnAssetCreate(string path, Bundle asset)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetCreate(path, asset);
            }
        }

        void IAssetLife<AssetHandle>.OnAssetRelease(AssetHandle asset, int count)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetRelease(asset, count);
            }
        }

        void IAssetLife<Bundle>.OnAssetRelease(Bundle asset, int count)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetRelease(asset, count);
            }
        }

        void IAssetLife<AssetHandle>.OnAssetRetain(AssetHandle asset, int count)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetRetain(asset, count);
            }
        }

        void IAssetLife<Bundle>.OnAssetRetain(Bundle asset, int count)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetRetain(asset, count);
            }
        }

        void IAssetLife<AssetHandle>.OnAssetUnload(string path, AssetHandle asset)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetUnload(path, asset);
            }
        }

        void IAssetLife<Bundle>.OnAssetUnload(string path, Bundle asset)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetUnload(path, asset);
            }
        }


    }

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


                var bundle_map = AssetsHelper.GetOrDefaultFromDictionary(map, bundleName);
                if (bundle_map == null)
                {
                    map.Add(bundleName, bundle);
                    await bundle;
                    length += bundle.length;
                }
                else
                    list.Remove(bundleName);
                list.AddLast(bundleName);

                if (!Application.isPlaying) break;
                if (length > maxLength)
                {
                    string _bundleName = list.First.Value;
                    Bundle _bundle = map[_bundleName];
                    ReleaseBundle(_bundle);
                    length -= _bundle.length;
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


        void IAssetLife<Bundle>.OnAssetUnload(string path, Bundle asset)
        {
        }
    }

}

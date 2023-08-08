using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WooAsset
{
    public partial class Assets
    {
        private static List<IAssetBridge> m_Assets = new List<IAssetBridge>();
        public static void ReleaseUselessBridges()
        {
            m_Assets.RemoveAll(x =>
            {
                var could = x.CouldRelease();
                if (could)
                    x.Release();
                return could;
            });

        }
        public static void AddBridge<T>(AssetBridge<T> asset) where T : class
        {
            m_Assets.Add(asset);
        }
        public static void ReleaseBridge<T>(T context) where T : class
        {
            var find = m_Assets.Find(x =>
            {
                if ((x as AssetBridge<T>).context == context)
                {
                    var could = x.CouldRelease();
                    return could;
                }
                return false;
            });
            if (find == null) return;
            m_Assets.Remove(find);
            find.Release();
        }


        public static AssetsGroupOperation PrepareAssets(string[] paths) => new AssetsGroupOperation(paths);
        public static AssetsGroupOperation PrepareAssetsByTag(string tag) => new AssetsGroupOperation(Assets.GetTagAssetPaths(tag).ToArray());
        public static InstantiateObjectOperation InstantiateAsync(string path, Transform parent) => new InstantiateObjectOperation(path, parent);
        public static void Destroy(GameObject gameObject)
        {
            GameObject.Destroy(gameObject);
            Assets.ReleaseBridge(gameObject);
        }
    }
}

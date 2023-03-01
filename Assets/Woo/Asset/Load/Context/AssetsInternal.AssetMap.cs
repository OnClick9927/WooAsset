

using System.Collections.Generic;
using Object = UnityEngine.Object;
namespace WooAsset
{
    partial class AssetsInternal
    {
        private class AssetMap : NameMap<Asset, Object>
        {

            protected override Asset CreateNew(string name, IAssetArgs args)
            {
                if (args is AssetLoadArgs)
                    return AssetsInternal.CreateAsset(name, GetDpAssets(name), (AssetLoadArgs)args);
                if (args is SceneAssetLoadArgs)
                    return AssetsInternal.CreateSceneAsset(name, GetDpAssets(name), (SceneAssetLoadArgs)args);
                return null;
            }
            public Asset LoadAssetAsync(string path)
            {
                AssetLoadArgs args = new AssetLoadArgs(path);
                Asset asset = base.LoadAsync(path, args);
                return asset;
            }


            public SceneAsset LoadSceneAssetAsync(string path)
            {
                SceneAssetLoadArgs args = new SceneAssetLoadArgs(path);
                SceneAsset asset = this.LoadAsync(path, args) as SceneAsset;
                return asset;
            }
            public override void Release(string path)
            {
                Asset result = Find(path);
                if (result == null) return;
                ReleaseRef(result);
                ReleaseBundleByAssetPath(result.path);
                ReleaseDpAssets(result.path);
                TryRealUnload(path);
            }


            private List<Asset> GetDpAssets(string assetPath)
            {
                List<Asset> result = null;
                var paths = GetAssetDps(assetPath);
                if (paths != null)
                {
                    result = new List<Asset>();
                    foreach (var path in paths)
                    {
                        Asset asset = Find(path);
                        if (asset != null)
                        {
                            result.Add(asset);
                        }
                    }
                }
                return result;
            }
            private void ReleaseDpAssets(string assetPath)
            {
                var paths = GetAssetDps(assetPath);
                if (paths != null)
                {
                    foreach (var path in paths)
                    {
                        Release(path);
                    }
                }
            }


        }
    }
}

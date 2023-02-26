

using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private class NomalAssetMode : IAssetMode
        {
    

            public Asset CreateAsset(string assetPath, List<Asset> dps, AssetLoadArgs arg)
            {
                return new Asset(LoadBundleByAssetPath(assetPath), dps, arg);
            }

            public IReadOnlyList<string> GetAllAssetPaths()
            {
                return IsManifestNull() ? null : manifest.GetAssets();
            }

            public SceneAsset CreateSceneAsset(string assetPath, List<Asset> dps, SceneAssetLoadArgs arg)
            {
                return new SceneAsset(LoadBundleByAssetPath(assetPath), dps, arg);
            }

            public IReadOnlyList<string> GetTagAssetPaths(string tag)
            {
                return IsManifestNull() ? null : manifest.GetTagAssetPaths(tag);
            }

            public string GetAssetTag(string assetPath)
            {
                return IsManifestNull() ? null : manifest.GetAssetTag(assetPath);

            }
        }
    }
}

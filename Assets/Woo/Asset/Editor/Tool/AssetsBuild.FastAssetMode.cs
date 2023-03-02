using System.Collections.Generic;
namespace WooAsset
{
    partial class AssetsBuild
    {

        public class FastAssetMode : IAssetMode
        {
            public Asset CreateAsset(string assetPath, List<Asset> dps, AssetLoadArgs arg)
            {
                return new EditorAsset(arg);
            }

            public IReadOnlyList<string> GetAllAssetPaths()
            {
                List<string> list = new List<string>();
                cache.GetAssets().ForEach(asset =>
                {
                    if (asset.type != AssetInfo.AssetType.Directory)
                    {
                        list.Add(asset.path);
                    }
                });
                cache.GetSingleFiles().ForEach(asset =>
                {
                    list.Add(asset.path);
                });
                return list;
            }

            public SceneAsset CreateSceneAsset(string assetPath, List<Asset> dps, SceneAssetLoadArgs arg)
            {
                return new EditorSceneAsset(arg);
            }

            public IReadOnlyList<string> GetTagAssetPaths(string tag)
            {
                return cache.GetTagAssetPaths(tag);
            }

            public string GetAssetTag(string assetPath)
            {
                return cache.GetAssetTag(assetPath);
            }

            public IReadOnlyList<string> GetAllTags()
            {
                return setting.tags;
            }
        }
    }

}

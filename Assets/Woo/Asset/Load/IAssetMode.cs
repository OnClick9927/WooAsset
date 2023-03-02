
using System.Collections.Generic;
namespace WooAsset
{
    public interface IAssetMode
    {
        Asset CreateAsset(string assetPath, List<Asset> dps, AssetLoadArgs arg);
        SceneAsset CreateSceneAsset(string assetPath, List<Asset> dps, SceneAssetLoadArgs arg);
        IReadOnlyList<string> GetAllAssetPaths();
        IReadOnlyList<string> GetTagAssetPaths(string tag);
        IReadOnlyList<string> GetAllTags();

        string GetAssetTag(string assetPath);
    }
}

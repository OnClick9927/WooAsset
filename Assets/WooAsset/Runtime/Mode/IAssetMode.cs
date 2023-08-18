
using System.Collections.Generic;

namespace WooAsset
{
    public interface IAssetMode
    {
        bool Initialized();
        Operation InitAsync(string version, bool again, string[] tags);

        UnzipRawFileOperation UnzipRawFile();

        AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg);
        IReadOnlyList<string> GetAllAssetPaths();
        IReadOnlyList<string> GetTagAssetPaths(string tag);
        IReadOnlyList<string> GetAllTags();


        IReadOnlyList<string> GetAssetTags(string assetPath);
        IReadOnlyList<string> GetAssetDependencies(string assetPath);

        IReadOnlyList<string> GetAllAssetPaths(string bundleName);
        CheckBundleVersionOperation VersionCheck();

        IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result);

        CopyStreamBundlesOperation CopyToSandBox(string from, string to);
        AssetType GetAssetType(string assetPath);

        bool ContainsAsset(string assetPath);

    }
}

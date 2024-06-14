using System.Collections.Generic;
using System;

namespace WooAsset
{
    public interface IAssetMode
    {
        string version { get; }
        bool Initialized();
        Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        CheckBundleVersionOperation LoadRemoteVersions();
        Operation CopyToSandBox(string from, string to);
        Bundle CreateBundle(string bundleName, BundleLoadArgs args);
        VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType);
        AssetData GetAssetData(string assetPath);
        BundleData GetBundleData(string bundleName);
        IReadOnlyList<string> GetAllAssetPaths();
        IReadOnlyList<string> GetTagAssetPaths(string tag);
        IReadOnlyList<string> GetAllTags();
        IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result);
        IReadOnlyList<string> GetAllAssetPaths(string bundleName);
    }
}

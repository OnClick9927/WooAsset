using System.Collections.Generic;
using System;

namespace WooAsset
{
    public abstract class AssetMode : IAssetMode
    {
        Operation IAssetMode.InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs)
        {
            SetVersion(version);
            return InitAsync(version, again, getPkgs);
        }
        bool IAssetMode.Initialized() => Initialized();
        Operation IAssetMode.CopyToSandBox(string from, string to) => CopyToSandBox(from, to);
        CheckBundleVersionOperation IAssetMode.LoadRemoteVersions() => LoadRemoteVersions();
        Bundle IAssetMode.CreateBundle(string bundleName, BundleLoadArgs args) => CreateBundle(bundleName, args);
        VersionCompareOperation IAssetMode.CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) => CompareVersion(version, pkgs, compareType);
        protected virtual ManifestData manifest { get; }

        public string version { get; private set; }
        protected void SetVersion(string version) => (this).version = version;
        protected abstract bool Initialized();
        protected abstract Operation CopyToSandBox(string from, string to);
        protected abstract Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        protected abstract CheckBundleVersionOperation LoadRemoteVersions();
        protected abstract Bundle CreateBundle(string bundleName, BundleLoadArgs args);
        protected abstract VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType);

        AssetData IAssetMode.GetAssetData(string assetPath) => GetAssetData(assetPath);
        BundleData IAssetMode.GetBundleData(string bundleName) => GetBundleData(bundleName);
        IReadOnlyList<string> IAssetMode.GetAllAssetPaths() => GetAllAssetPaths();
        IReadOnlyList<string> IAssetMode.GetTagAssetPaths(string tag) => GetTagAssetPaths(tag);
        IReadOnlyList<string> IAssetMode.GetAllTags() => GetAllTags();
        IReadOnlyList<string> IAssetMode.GetAssetsByAssetName(string name, List<string> result) => GetAssetsByAssetName(name, result);
        IReadOnlyList<string> IAssetMode.GetAllAssetPaths(string bundleName) => GetAllAssetPaths(bundleName);

        protected virtual AssetData GetAssetData(string assetPath) => manifest.GetAssetData(assetPath);
        protected virtual BundleData GetBundleData(string bundleName) => manifest.GetBundleData(bundleName);
        protected virtual IReadOnlyList<string> GetAllAssetPaths() => manifest.allPaths;
        protected virtual IReadOnlyList<string> GetTagAssetPaths(string tag) => manifest.GetTagAssetPaths(tag);
        protected virtual IReadOnlyList<string> GetAllTags() => manifest.allTags;
        protected virtual IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => manifest.GetAssetsByAssetName(name, result);

        protected virtual IReadOnlyList<string> GetAllAssetPaths(string bundleName) => manifest.GetAssets(bundleName);




    }

}

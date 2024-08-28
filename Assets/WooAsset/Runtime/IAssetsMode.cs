using System.Collections.Generic;
using System;
using System.Linq;

namespace WooAsset
{
    public interface IAssetsMode
    {
        string version { get; }
        bool Initialized();
        Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        LoadRemoteVersionsOperation LoadRemoteVersions();
        Operation CopyToSandBox(string from, string to);
        Bundle CreateBundle(string bundleName, BundleLoadArgs args);
        VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType);
        AssetData GetAssetData(string assetPath);
        AssetData GetFuzzyAssetData(string path);
        BundleData GetBundleData(string bundleName);
        IReadOnlyList<string> GetAllAssetPaths();
        IReadOnlyList<string> GetTagAssetPaths(string tag);
        IReadOnlyList<string> GetAllTags();
        IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result);
        IReadOnlyList<string> GetAllAssetPaths(string bundleName);
    }


    public abstract class AssetsMode : IAssetsMode
    {
        Operation IAssetsMode.InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs)
        {
            SetVersion(version);
            return InitAsync(version, again, getPkgs);
        }
        bool IAssetsMode.Initialized() => Initialized();
        Operation IAssetsMode.CopyToSandBox(string from, string to) => CopyToSandBox(from, to);
        LoadRemoteVersionsOperation IAssetsMode.LoadRemoteVersions() => LoadRemoteVersions();
        Bundle IAssetsMode.CreateBundle(string bundleName, BundleLoadArgs args) => CreateBundle(bundleName, args);
        VersionCompareOperation IAssetsMode.CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) => CompareVersion(version, pkgs, compareType);
        protected virtual ManifestData manifest { get; }

        public string version { get; private set; }
        protected void SetVersion(string version) => (this).version = version;
        protected abstract bool Initialized();
        protected abstract Operation CopyToSandBox(string from, string to);
        protected abstract Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        protected abstract LoadRemoteVersionsOperation LoadRemoteVersions();
        protected abstract Bundle CreateBundle(string bundleName, BundleLoadArgs args);
        protected abstract VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType);

        AssetData IAssetsMode.GetAssetData(string assetPath) => GetAssetData(assetPath);
        BundleData IAssetsMode.GetBundleData(string bundleName) => GetBundleData(bundleName);
        IReadOnlyList<string> IAssetsMode.GetAllAssetPaths() => GetAllAssetPaths();
        IReadOnlyList<string> IAssetsMode.GetTagAssetPaths(string tag) => GetTagAssetPaths(tag);
        IReadOnlyList<string> IAssetsMode.GetAllTags() => GetAllTags();
        IReadOnlyList<string> IAssetsMode.GetAssetsByAssetName(string name, List<string> result) => GetAssetsByAssetName(name, result);
        IReadOnlyList<string> IAssetsMode.GetAllAssetPaths(string bundleName) => GetAllAssetPaths(bundleName);
        AssetData IAssetsMode.GetFuzzyAssetData(string path) => GetFuzzyAssetData(path);
        protected virtual AssetData GetAssetData(string assetPath) => manifest.GetAssetData(assetPath);
        protected virtual AssetData GetFuzzyAssetData(string path)=> manifest.GetFuzzyAssetData(path);
        protected virtual BundleData GetBundleData(string bundleName) => manifest.GetBundleData(bundleName);
        protected virtual IReadOnlyList<string> GetAllAssetPaths() => manifest.allPaths;
        protected virtual IReadOnlyList<string> GetTagAssetPaths(string tag) => manifest.GetTagAssetPaths(tag);
        protected virtual IReadOnlyList<string> GetAllTags() => manifest.allTags;
        protected virtual IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => manifest.GetAssetsByAssetName(name, result);

        protected virtual IReadOnlyList<string> GetAllAssetPaths(string bundleName) => manifest.GetAssets(bundleName);


    }


    public class NormalAssetsMode : AssetsMode
    {

        private LoadManifestOperation manifestOp;

        protected override ManifestData manifest => Initialized() ? manifestOp.manifest : null;

        protected override bool Initialized()
        {
            if (manifestOp == null) return false;
            return manifestOp.isDone;
        }

        protected override Operation CopyToSandBox(string from, string to) => new CopyStreamBundlesOperation(from, to);

        protected override Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs)
        {
            if (again)
            {
                manifestOp = null;
                AssetsInternal.UnloadBundles();
                AssetsHelper.Log($"Still exist {AssetsInternal.GetLoadedBundleCount()} bundles when init again");
            }
            if (manifestOp == null)
                manifestOp = new LoadManifestOperation(AssetsInternal.GetLoadedBundleNames().ToList()
                    , version, getPkgs);
            if (manifestOp.isDone)
                ManifestOp_completed();
            else
                manifestOp.completed += ManifestOp_completed;
            return manifestOp;
        }

        private void ManifestOp_completed()
        {
            SetVersion(manifestOp.GetVersion());
        }

        protected override LoadRemoteVersionsOperation LoadRemoteVersions() => new LoadRemoteVersionsOperation();

        protected override Bundle CreateBundle(string bundleName, BundleLoadArgs args) => new Bundle(args);

        protected override VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) => new VersionCompareOperation(version, pkgs, compareType);
    }


}

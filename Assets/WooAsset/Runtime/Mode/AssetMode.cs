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
        CopyStreamBundlesOperation IAssetMode.CopyToSandBox(string from, string to) => CopyToSandBox(from, to);
        CheckBundleVersionOperation IAssetMode.LoadRemoteVersions() => LoadRemoteVersions();
        Bundle IAssetMode.CreateBundle(string bundleName, BundleLoadArgs args) => CreateBundle(bundleName, args);
        VersionCompareOperation IAssetMode.CompareVersion(VersionData version, List<PackageData> pkgs) => CompareVersion(version, pkgs);
        public abstract ManifestData manifest { get; }

        public string version { get; private set; }
        protected void SetVersion(string version) => this.version = version;
        protected abstract bool Initialized();
        protected abstract CopyStreamBundlesOperation CopyToSandBox(string from, string to);
        protected abstract Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        protected abstract CheckBundleVersionOperation LoadRemoteVersions();
        protected abstract Bundle CreateBundle(string bundleName, BundleLoadArgs args);
        protected abstract VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs);

    }

}

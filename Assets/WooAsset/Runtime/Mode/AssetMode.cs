using static WooAsset.AssetsVersionCollection.VersionData;
using static WooAsset.AssetsVersionCollection;
using System.Collections.Generic;
using System;

namespace WooAsset
{
    public abstract class AssetMode : IAssetMode
    {
        bool IAssetMode.Initialized() => Initialized();
        CopyStreamBundlesOperation IAssetMode.CopyToSandBox(string from, string to) => CopyToSandBox(from, to);
        Operation IAssetMode.InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs) => InitAsync(version, again, getPkgs);
        CheckBundleVersionOperation IAssetMode.VersionCheck() => VersionCheck();
        Bundle IAssetMode.CreateBundle(string bundleName, BundleLoadArgs args) => CreateBundle(bundleName, args);
        public abstract ManifestData manifest { get; }
        protected abstract bool Initialized();
        protected abstract CopyStreamBundlesOperation CopyToSandBox(string from, string to);
        protected abstract Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        protected abstract CheckBundleVersionOperation VersionCheck();
        protected abstract Bundle CreateBundle(string bundleName, BundleLoadArgs args);
    }

}

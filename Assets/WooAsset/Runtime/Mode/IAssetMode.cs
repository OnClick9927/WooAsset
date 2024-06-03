using System.Collections.Generic;
using System;

namespace WooAsset
{
    public interface IAssetMode
    {
        ManifestData manifest { get; }
        string version { get; }
        bool Initialized();
        Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        CheckBundleVersionOperation LoadRemoteVersions();
        Operation CopyToSandBox(string from, string to);
        Bundle CreateBundle(string bundleName, BundleLoadArgs args);
        VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType);
    }
}

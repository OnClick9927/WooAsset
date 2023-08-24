

using System.Collections.Generic;

namespace WooAsset
{
    public abstract class AssetMode : IAssetMode
    {
        bool IAssetMode.Initialized() => Initialized();
        UnzipRawFileOperation IAssetMode.UnzipRawFile() => new UnzipRawFileOperation(manifest.rawAssets_copy);
        IReadOnlyList<string> IAssetMode.GetAllAssetPaths()
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifest.GetAssets();
        }
        bool IAssetMode.ContainsAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            if (!((IAssetMode)this).Initialized())
                return false;
            return manifest.ContainsAsset(assetPath);
        }
        IReadOnlyList<string> IAssetMode.GetAllAssetPaths(string bundleName)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifest.GetAssets(bundleName);
        }
        IReadOnlyList<string> IAssetMode.GetAssetsByAssetName(string name, List<string> result)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifest.GetAssetsByAssetName(name, result);
        }
        IReadOnlyList<string> IAssetMode.GetAllTags()
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifest.GetAllTags();
        }
        IReadOnlyList<string> IAssetMode.GetAssetDependencies(string assetPath)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifest.GetAssetDependencies(assetPath);
        }
        IReadOnlyList<string> IAssetMode.GetAssetTags(string assetPath)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifest.GetAssetTags(assetPath);
        }
        AssetType IAssetMode.GetAssetType(string assetPath)
        {
            if (!((IAssetMode)this).Initialized())
                return AssetType.None;
            return manifest.GetAssetType(assetPath);
        }
        IReadOnlyList<string> IAssetMode.GetTagAssetPaths(string tag)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifest.GetTagAssetPaths(tag);
        }
        CopyStreamBundlesOperation IAssetMode.CopyToSandBox(string from, string to) => CopyToSandBox(from, to);
        AssetHandle IAssetMode.CreateAsset(string assetPath, AssetLoadArgs arg) => CreateAsset(assetPath, arg);
        Operation IAssetMode.InitAsync(string version, bool again, string[] tags) => InitAsync(version, again, tags);
        CheckBundleVersionOperation IAssetMode.VersionCheck() => VersionCheck();

        protected abstract ManifestData manifest { get; }
        protected abstract bool Initialized();
        protected abstract CopyStreamBundlesOperation CopyToSandBox(string from, string to);
        protected abstract AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg);
        protected abstract Operation InitAsync(string version, bool again, string[] tags);
        protected abstract CheckBundleVersionOperation VersionCheck();
    }

}

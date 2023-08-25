namespace WooAsset
{
    public abstract class AssetMode : IAssetMode
    {
        bool IAssetMode.Initialized() => Initialized();
        UnzipRawFileOperation IAssetMode.UnzipRawFile() => new UnzipRawFileOperation(manifest.rawAssets_copy);
     

        CopyStreamBundlesOperation IAssetMode.CopyToSandBox(string from, string to) => CopyToSandBox(from, to);
        AssetHandle IAssetMode.CreateAsset(string assetPath, AssetLoadArgs arg) => CreateAsset(assetPath, arg);
        Operation IAssetMode.InitAsync(string version, bool again, string[] tags) => InitAsync(version, again, tags);
        CheckBundleVersionOperation IAssetMode.VersionCheck() => VersionCheck();

        public abstract ManifestData manifest { get; }
        protected abstract bool Initialized();
        protected abstract CopyStreamBundlesOperation CopyToSandBox(string from, string to);
        protected abstract AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg);
        protected abstract Operation InitAsync(string version, bool again, string[] tags);
        protected abstract CheckBundleVersionOperation VersionCheck();

     
    }

}

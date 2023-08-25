namespace WooAsset
{
    public interface IAssetMode
    {
        ManifestData manifest { get; }
        bool Initialized();
        Operation InitAsync(string version, bool again, string[] tags);
        UnzipRawFileOperation UnzipRawFile();
        AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg);
        CheckBundleVersionOperation VersionCheck();
        CopyStreamBundlesOperation CopyToSandBox(string from, string to);
    }
}

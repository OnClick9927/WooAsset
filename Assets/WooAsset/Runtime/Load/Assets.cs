using System.Collections.Generic;

namespace WooAsset
{
    public partial class Assets
    {
        public static void SetAssetsSetting(AssetsSetting setting) => AssetsInternal.SetAssetsSetting(setting);
        public static CheckBundleVersionOperation VersionCheck() => AssetsInternal.VersionCheck();
        public static DownLoadBundleOperation DownLoadBundle(string bundleName) => new DownLoadBundleOperation(bundleName);
        public static CopyBundleOperation CopyToSandBox() => AssetsInternal.CopyToSandBox();
        public static bool Initialized() => AssetsInternal.Initialized();
        public static AssetOperation InitAsync(string version = "", bool again = false, params string[] tags) => AssetsInternal.InitAsync(version, again, tags);
        public static UnzipRawFileOperation UnzipRawFile() => AssetsInternal.UnzipRawFile();
        public static Asset LoadAssetAsync(string path) => AssetsInternal.LoadAsset(path, true, true) as Asset;
        public static Asset LoadAsset(string path) => AssetsInternal.LoadAsset(path, false, true) as Asset;
        public static SceneAsset LoadSceneAssetAsync(string path) => AssetsInternal.LoadAsset(path, true, false) as SceneAsset;
        public static SceneAsset LoadSceneAsset(string path) => AssetsInternal.LoadAsset(path, false, false) as SceneAsset;
        public static Asset LoadFileAsset(string path) => AssetsInternal.LoadFileAsset(path, false);
        public static Asset LoadFileAssetAsync(string path) => AssetsInternal.LoadFileAsset(path, true);

        public static void Release(AssetHandle asset) => AssetsInternal.Release(asset.path);
        public static void Release(string assetPath) => AssetsInternal.Release(assetPath);

        public static void UnloadBundles() => AssetsInternal.UnloadBundles();
        public static bool GetIsAssetLoaded(string assetPath) => AssetsInternal.GetIsAssetLoaded(assetPath);
        public static string GetRawFileToDlcPath(string path) => AssetsInternal.GetRawFileToDlcPath(path);



        public static IReadOnlyList<string> GetAssetTags(string assetPath) => AssetsInternal.GetAssetTags(assetPath);
        public static IReadOnlyList<string> GetAllAssetPaths() => AssetsInternal.GetAllAssetPaths();
        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => AssetsInternal.GetTagAssetPaths(tag);
        public static IReadOnlyList<string> GetAllTags() => AssetsInternal.GetAllTags();

        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => AssetsInternal.GetAllAssetPaths(bundleName);

    }
}

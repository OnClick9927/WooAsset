using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WooAsset
{
    static partial class AssetsInternal
    {

        private static BundleMap bundles;
        private static AssetMap assets;
        private static AssetsSetting setting;

        private static MixedAssetLife mixedlife;
        private static IAssetsMode _defaultMode = new NormalAssetsMode();
        public static IAssetsMode mode { get; set; }

        public static bool isNormalMode => mode != null && mode is NormalAssetsMode;
        private static string localSaveDir;


        static AssetsInternal()
        {
            mode = _defaultMode;
            bundles = new BundleMap();
            assets = new AssetMap();
            SetLocalSaveDir(AssetsHelper.CombinePath(Application.persistentDataPath, "DLC"));
        }
        public static void SetLocalSaveDir(string path)
        {
            localSaveDir = path;
            AssetsHelper.CreateDirectory(localSaveDir);
        }
        public static string GetLocalSaveDir() => localSaveDir;
        public static void AddAssetLife(IAssetLife life)
        {
            if (mixedlife == null)
            {
                mixedlife = new MixedAssetLife();
                bundles.SetListen(mixedlife);
                assets.SetListen(mixedlife);
            }
            mixedlife.AddLife(life);
        }
        public static void RemoveAssetLife(IAssetLife life)
        {
            if (mixedlife == null) return;
            mixedlife.RemoveAssetLife(life);
        }

        public static void SetAssetsSetting(AssetsSetting setting)
        {
            AssetsInternal.setting = setting;
            IAssetLife life = setting.GetAssetLife();
            if (life != null)
                AddAssetLife(life);
            DownLoader.RequestCountAtSameTime = setting.GetWebRequestCountAtSameTime();
        }
        private static int GetWebRequestTimeout() => setting.GetWebRequestTimeout();
        private static int GetWebRequestRetryCount() => setting.GetWebRequestRetryCount();

        private static string GetUrlFromBundleName(string bundleName) => setting.GetUrlByBundleName(AssetsHelper.buildTarget, bundleName);

        public static string GetUrlFromBundleName(string version, string bundleName) => setting.GetUrlByBundleName(AssetsHelper.buildTarget, version, bundleName);

        private static bool GetFuzzySearch() => setting.GetFuzzySearch();
        private static FileNameSearchType GetFileNameSearchType() => setting.GetFileNameSearchType();


        private static bool GetAutoUnloadBundle() => setting.GetAutoUnloadBundle();



        private static IAssetEncrypt GetEncrypt(int enCode) => setting.GetEncrypt(enCode);
        public static bool GetSaveBytesWhenPlaying() => setting.GetSaveBytesWhenPlaying() && !GetBundleAlwaysFromWebRequest();
        public static bool GetCachesDownloadedBundles() => setting.GetCachesDownloadedBundles();

        public static bool GetBundleAlwaysFromWebRequest() => setting.GetBundleAlwaysFromWebRequest();

        public static bool CheckVersionByVersionCollection() => setting.CheckVersionByVersionCollection();
        public static long GetLoadingMaxTimeSlice() => setting.GetLoadingMaxTimeSlice();
        public static bool NeedCopyStreamBundles() => setting.NeedCopyStreamBundles();
        public static string GetStreamingFileUrl(string url) => setting.GetStreamingFileUrl(url);

    }


    partial class AssetsInternal
    {
        public static bool Initialized() => mode.Initialized();
        public static Operation InitAsync(string version, bool ignoreLocalVersion, bool again, Func<VersionData, List<PackageData>> getPkgs) => mode.InitAsync(version, ignoreLocalVersion, again, GetFuzzySearch(), GetFileNameSearchType(), getPkgs);
        public static string GetVersion() => Initialized() ? mode.version : string.Empty;



        public static AssetData GetAssetData(string assetPath)
        {
            var data = mode.GetAssetData(assetPath);
            if (data == null && GetFuzzySearch())
            {
                data = mode.GetFuzzyAssetData(assetPath);
            }
            return data;
        }
        public static BundleData GetBundleData(string bundleName) => mode.GetBundleData(bundleName);
        private static Bundle CreateBundle(BundleLoadArgs args) => mode.CreateBundle(args.bundleName, args);
        public static AssetHandle LoadAsset(string path, bool sub, bool async, Type type)
        {
            var data = GetAssetData(AssetsHelper.ToRegularPath(path));
            if (data == null)
            {
                AssetsHelper.LogError($"Not Found Asset: {path}");
                return null;
            }
            return assets.LoadAsset(data, async, type, sub);
        }
        public static AsyncOperation UnloadSceneAsync(string path, UnloadSceneOptions op)
        {
            var scene = assets.Find(path) as SceneAsset;
            var _op = scene.UnloadSceneAsync(op);
            Release(path);
            return _op;
        }

        public static void Release(string assetPath) => assets.Release(assetPath);
        public static void UnloadBundles() => bundles.UnloadBundles();

        public static string GetBundleLocalPath(string bundleName) => setting.GetBundleLocalPath(AssetsHelper.CombinePath(localSaveDir, bundleName));



        public static bool GetIsAssetLoaded(string assetPath) => assets.Find(assetPath) != null;
        public static Operation CopyToSandBox() => mode.CopyToSandBox(AssetsHelper.StreamBundlePath, localSaveDir);
        public static string GUIDToAssetPath(string guid) => mode.GUIDToAssetPath(guid);
        public static int GetLoadedBundleCount() => bundles.GetCount();
        public static IEnumerable<string> GetLoadedBundleNames() => bundles.GetKeys();
        public static IReadOnlyList<string> GetAssetTags(string assetPath) => GetAssetData(assetPath)?.tags;
        public static IReadOnlyList<string> GetAllAssetPaths() => mode.GetAllAssetPaths();
        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => mode.GetTagAssetPaths(tag);
        public static IReadOnlyList<string> GetAllTags() => mode.GetAllTags();
        public static IReadOnlyList<string> GetAssetsByAssetName(string name) => mode.GetAssetsByAssetName(name);
        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => mode.GetAllAssetPaths(bundleName);


    }

    partial class AssetsInternal
    {
        public static LoadRemoteVersionsOperation LoadRemoteVersions() => mode.LoadRemoteVersions();

        public static LoadVersionDataOperation DownloadVersionData(string version) => new LoadVersionDataOperation(version);
        public static VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) => mode.CompareVersion(version, pkgs, compareType);

        public static BytesDownLoader DownLoadRawBundle(string version, string bundleName) => DownloadBytes(GetUrlFromBundleName(version, bundleName));
        public static BundleDownLoader DownLoadBundle(string version, string bundleName, uint crc, Hash128 hash)
        {
            return DownLoaderSystem.Bundle(GetUrlFromBundleName(version, bundleName), AssetsInternal.GetCachesDownloadedBundles(), crc, hash, GetWebRequestTimeout(), GetWebRequestRetryCount());
        }

        public static DownLoader DownLoadBundleFile(string version, string bundleName) =>
            DownLoadFile(GetUrlFromBundleName(version, bundleName), GetBundleLocalPath(bundleName));

        public static BytesDownLoader DownloadVersion(string version, string bundleName) => DownLoadRawBundle(version, bundleName);
        public static BytesDownLoader DownloadRemoteVersion() =>
            DownloadBytes(GetUrlFromBundleName(AssetsHelper.VersionCollectionName));
        public static DownLoader DownLoadFile(string url, string path) => DownLoaderSystem.File(url, path, GetWebRequestTimeout(), GetWebRequestRetryCount());
        public static BytesDownLoader DownloadBytes(string url) => DownLoaderSystem.Bytes(url, GetWebRequestTimeout(), GetWebRequestRetryCount());
    }

}

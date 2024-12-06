using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WooAsset
{
    public static partial class AssetsInternal
    {

        private static BundleMap bundles;
        private static AssetMap assets;
        private static AssetsSetting setting;

        private static MixedAssetLife mixedlife;
        private static IAssetsMode _defaultMode = new NormalAssetsMode();
        public static IAssetsMode mode { get; set; }
        private static string localSaveDir;

        public static void SetLocalSaveDir(string path)
        {
            localSaveDir = path;
            AssetsHelper.CreateDirectory(localSaveDir);
        }
        public static string GetLocalSaveDir() => localSaveDir;
        static AssetsInternal()
        {
            mode = _defaultMode;
            bundles = new BundleMap();
            assets = new AssetMap();
            SetLocalSaveDir(AssetsHelper.CombinePath(Application.persistentDataPath, "DLC"));
        }

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


    }
    partial class AssetsInternal
    {
        private static Bundle CreateBundle(BundleLoadArgs args) => mode.CreateBundle(args.bundleName, args);
        public static bool Initialized() => mode.Initialized();
        public static Operation InitAsync(string version, bool ignoreLoalVersion, bool again, Func<VersionData, List<PackageData>> getPkgs) => mode.InitAsync(version, ignoreLoalVersion, again, GetFuzzySearch(), getPkgs);
        public static LoadRemoteVersionsOperation LoadRemoteVersions() => mode.LoadRemoteVersions();
        public static Operation CopyToSandBox(bool again) => mode.CopyToSandBox(AssetsHelper.ToAbsPath(AssetsHelper.StreamBundlePath), localSaveDir, again);



        public static AssetType GetAssetType(string assetPath) => Initialized() ? GetAssetData(assetPath).type : AssetType.None;
        public static AssetData GetAssetData(string assetPath)
        {
            var data = mode.GetAssetData(assetPath);
            if (data == null && GetFuzzySearch())
            {
                data = mode.GetFuzzyAssetData(assetPath);
            }
            return data;
        }
        public static string GUIDToAssetPath(string guid) => mode.GUIDToAssetPath(guid);
        public static BundleData GetBundleData(string bundleName) => mode.GetBundleData(bundleName);
        public static string GetVersion() => Initialized() ? mode.version : string.Empty;
        public static IReadOnlyList<string> GetAssetTags(string assetPath) => GetAssetData(assetPath)?.tags;



        public static IReadOnlyList<string> GetAllAssetPaths() => mode.GetAllAssetPaths();
        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => mode.GetTagAssetPaths(tag);
        public static IReadOnlyList<string> GetAllTags() => mode.GetAllTags();
        public static IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => mode.GetAssetsByAssetName(name, result);
        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => mode.GetAllAssetPaths(bundleName);


    }
    partial class AssetsInternal
    {
        private static int GetWebRequestTimeout() => setting.GetWebRequestTimeout();
        private static int GetWebRequestRetryCount() => setting.GetWebRequestRetryCount();
        private static string GetUrlFromBundleName(string bundleName) => setting.GetUrlByBundleName(AssetsHelper.buildTarget, bundleName);

        private static string GetUrlFromBundleName(string version, string bundleName) => setting.GetUrlByBundleName(AssetsHelper.buildTarget, version, bundleName);

        private static bool GetFuzzySearch() => setting.GetFuzzySearch();

        private static bool GetAutoUnloadBundle() => setting.GetAutoUnloadBundle();


        public static void SetAssetsSetting(AssetsSetting setting)
        {
            AssetsInternal.setting = setting;
            IAssetLife life = setting.GetAssetLife();
            if (life != null)
                AddAssetLife(life);
        }

        public static DownLoader DownLoadRawBundle(string version, string bundleName) => DownloadBytes(GetUrlFromBundleName(version, bundleName));
        public static DownLoader DownLoadBundle(string version, string bundleName, uint crc, Hash128 hash)
        {
            return new BundleDownLoader(GetUrlFromBundleName(version, bundleName), AssetsInternal.GetCachesDownloadedBundles(), crc, hash, GetWebRequestTimeout(), GetWebRequestRetryCount());
        }

        public static DownLoader DownLoadBundleFile(string version, string bundleName) =>
            DownLoadFile(GetUrlFromBundleName(version, bundleName), GetBundleLocalPath(bundleName));

        public static DownLoader DownloadVersion(string version, string bundleName) => DownLoadRawBundle(version, bundleName);
        public static DownLoader DownloadRemoteVersion() =>
            DownloadBytes(GetUrlFromBundleName(AssetsHelper.VersionCollectionName));
        public static DownLoader DownLoadFile(string url, string path) => new FileDownLoader(url, path, GetWebRequestTimeout(), GetWebRequestRetryCount());
        public static DownLoader DownloadBytes(string url) => new DownLoader(url, GetWebRequestTimeout(), GetWebRequestRetryCount());




        public static LoadVersionDataOperation DownloadVersionData(string version) => new LoadVersionDataOperation(version);
        public static VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) => mode.CompareVersion(version, pkgs, compareType);


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
        public static string GetBundleLocalPath(string bundleName) => AssetsHelper.CombinePath(localSaveDir, bundleName);
        public static void UnloadBundles() => bundles.UnloadBundles();

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



        public static bool GetIsAssetLoaded(string assetPath) => assets.Find(assetPath) != null;



        public static void Release(string assetPath) => assets.Release(assetPath);



        public static int GetLoadedBundleCount() => bundles.GetCount();
        public static IEnumerable<string> GetLoadedBundleNames() => bundles.GetKeys();





    }
}

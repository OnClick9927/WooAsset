using System;
using System.Collections.Generic;
using UnityEngine;
using static WooAsset.AssetsVersionCollection.VersionData;
using static WooAsset.AssetsVersionCollection;

namespace WooAsset
{
    public static partial class AssetsInternal
    {

        private static BundleMap bundles;
        private static AssetMap assets;
        private static AssetsSetting setting;

        private static MixedAssetLife mixedlife;
        private static IAssetMode _defaultMode = new NormalAssetMode();
        public static IAssetMode mode { get; set; }
        private static string localSaveDir;

        private static string localRfcDir { get { return AssetsHelper.CombinePath(localSaveDir, "rfc"); } }


        public static void SetLocalSaveDir(string path)
        {
            localSaveDir = path;
            AssetsHelper.CreateDirectory(localSaveDir);
            AssetsHelper.CreateDirectory(localRfcDir);
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
        private static AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg) => mode.CreateAsset(assetPath, arg);
        public static bool Initialized() => mode.Initialized();
        public static Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs) => mode.InitAsync(version, again, getPkgs);
        public static CheckBundleVersionOperation VersionCheck() => mode.VersionCheck();
        public static CopyStreamBundlesOperation CopyToSandBox() => mode.CopyToSandBox(AssetsHelper.streamBundleDirectory, localSaveDir);
        public static UnzipRawFileOperation UnzipRawFile() => mode.UnzipRawFile();



        public static AssetType GetAssetType(string assetPath) => Initialized() ? mode.manifest.GetAssetType(assetPath) : AssetType.None;
        public static ManifestData.AssetData GetAssetData(string assetPath) => Initialized() ? mode.manifest.GetAssetData(assetPath) : null;
        public static IReadOnlyList<string> GetAssetTags(string assetPath) => GetAssetData(assetPath)?.tags;
        public static string GetAssetBundleName(string assetPath) => Initialized() ? mode.manifest.GetAssetBundleName(assetPath) : string.Empty;


        public static IReadOnlyList<string> GetAllAssetPaths() => Initialized() ? mode.manifest.allPaths : null;
        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => Initialized() ? mode.manifest.GetTagAssetPaths(tag) : null;
        public static IReadOnlyList<string> GetAllTags() => Initialized() ? mode.manifest.allTags : null;
        public static IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => Initialized() ? mode.manifest.GetAssetsByAssetName(name, result) : null;
        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => Initialized() ? mode.manifest.GetAssets(bundleName) : null;


    }
    partial class AssetsInternal
    {
        private static int GetWebRequestTimeout() => setting.GetWebRequestTimeout();
        private static int GetWebRequestRetryCount() => setting.GetWebRequestRetryCount();
        private static string GetUrlFromBundleName(string bundleName) => setting.GetUrlByBundleName(AssetsHelper.buildTarget, bundleName);
        private static bool GetAutoUnloadBundle() => setting.GetAutoUnloadBundle();


        public static void SetAssetsSetting(AssetsSetting setting)
        {
            AssetsInternal.setting = setting;
            IAssetLife life = setting.GetAssetLife();
            if (life != null)
                AddAssetLife(life);
        }
        public static BundleDownloader2 DownLoadBundle(string bundleName) => new BundleDownloader2(GetUrlFromBundleName(bundleName), bundleName, GetWebRequestRetryCount(), GetWebRequestTimeout());

        public static BundleDownloader DownLoadBundleBytes(string bundleName) => new BundleDownloader(GetUrlFromBundleName(bundleName), bundleName, GetWebRequestRetryCount(), GetWebRequestTimeout());

        public static FileData.FileCompareType GetFileCheckType() => setting.GetFileCheckType();

        public static IAssetStreamEncrypt GetEncrypt() => setting.GetEncrypt();
        public static bool GetSaveBundlesWhenPlaying() => setting.GetSaveBundlesWhenPlaying() && !GetBundleAwalysFromWebRequest();
        public static bool GetBundleAwalysFromWebRequest() => setting.GetBundleAwalysFromWebRequest();

        public static long GetLoadingMaxTimeSlice() => setting.GetLoadingMaxTimeSlice();
        public static bool NeedCopyStreamBundles() => setting.NeedCopyStreamBundles();
        public static string OverwriteBundlePath(string bundlePath) => setting.OverwriteBundlePath(bundlePath);

    }
    partial class AssetsInternal
    {

        public static string GetRawFileToDlcPath(string path) => AssetsHelper.GetRawFileToDlcPath(localRfcDir, path);
        public static Downloader DownloadVersion(string bundleName) => new Downloader(GetUrlFromBundleName(bundleName), GetWebRequestTimeout(), GetWebRequestRetryCount());


        public static string GetBundleLocalPath(string bundleName) => OverwriteBundlePath(AssetsHelper.CombinePath(localSaveDir, bundleName));
        public static void UnloadBundles() => bundles.UnloadBundles();

        public static Bundle LoadBundle(string bundleName, bool async) => bundles.LoadAsync(new BundleLoadArgs(bundleName, async, GetEncrypt()));





        public static Asset LoadFileAsset(string path, bool async) => assets.LoadAssetAsync(new AssetLoadArgs(path, true, path.EndsWith("unity"), null, "", async)) as Asset;
        public static AssetHandle LoadAsset(string path, bool async)
        {
            assets.RemoveUselessAsset();
            var data = GetAssetData(AssetsHelper.ToRegularPath(path));
            if (data == null)
            {
                AssetsHelper.LogError($"Not Found Asset: {path}");
                return null;
            }
            List<AssetHandle> result = null;
            if (data.dps != null)
            {
                var find = assets.Find(data.path);
                if (find != null)
                    result = find.dps;
                else
                    result = new List<AssetHandle>();
                result.Clear();
                foreach (var item in data.dps)
                {
                    AssetHandle _asset = LoadAsset(item, async);
                    if (_asset != null)
                        result.Add(_asset);
                }
            }
            return assets.LoadAssetAsync(new AssetLoadArgs(data.path, false, data.path.EndsWith("unity"), result, "", async));
        }

        public static bool GetIsAssetLoaded(string assetPath) => assets.Find(assetPath) != null;



        public static void Release(string assetPath) => assets.Release(assetPath);



        public static IReadOnlyList<AssetHandle> GetLoadedAssets() => assets.GetAll();
        public static IReadOnlyList<Bundle> GetLoadedBundles() => bundles.GetAll();





    }
}

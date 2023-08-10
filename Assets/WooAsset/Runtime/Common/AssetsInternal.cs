using System.Collections.Generic;
using UnityEngine;

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

        public static string buildTarget
        {
            get
            {
#if UNITY_EDITOR
                switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
                {
                    case UnityEditor.BuildTarget.Android:
                        return "Android";
                    case UnityEditor.BuildTarget.StandaloneWindows:
                    case UnityEditor.BuildTarget.StandaloneWindows64:
                        return "Windows";
                    case UnityEditor.BuildTarget.iOS:
                        return "iOS";
                    case UnityEditor.BuildTarget.WebGL:
                        return "WebGL";
                    case UnityEditor.BuildTarget.StandaloneOSX:
                        return "OSX";
                }
#else
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer: return "Windows";
                    case RuntimePlatform.Android: return "Android";
                    case RuntimePlatform.IPhonePlayer: return "iOS";
                    case RuntimePlatform.WebGLPlayer: return "WebGL";
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.OSXEditor: return "OSX";

                }
#endif
                return string.Empty;
            }
        }

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
        public static IReadOnlyList<string> GetAllAssetPaths() => mode.GetAllAssetPaths();
        public static IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => mode.GetAssetsByAssetName(name, result);

        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => mode.GetTagAssetPaths(tag);
        public static IReadOnlyList<string> GetAllTags() => mode.GetAllTags();
        public static IReadOnlyList<string> GetAssetTags(string assetPath) => mode.GetAssetTags(assetPath);
        public static AssetType GetAssetType(string assetPath) => mode.GetAssetType(assetPath);
        private static IReadOnlyList<string> GetAssetDependencies(string assetPath) => mode.GetAssetDependencies(assetPath);
        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => mode.GetAllAssetPaths(bundleName);
        public static bool Initialized() => mode.Initialized();
        public static Operation InitAsync(string version, bool again, string[] tags) => mode.InitAsync(version, again, tags);
        public static CheckBundleVersionOperation VersionCheck() => mode.VersionCheck();
        public static CopyDirectoryOperation CopyToSandBox() => mode.CopyToSandBox(AssetsHelper.CombinePath(Application.streamingAssetsPath, buildTarget), localSaveDir, false);
        private static bool ContainsAsset(string assetPath) => mode.ContainsAsset(assetPath);
        public static UnzipRawFileOperation UnzipRawFile() => mode.UnzipRawFile();
    }
    partial class AssetsInternal
    {
        public static void SetAssetsSetting(AssetsSetting setting)
        {
            AssetsInternal.setting = setting;
            IAssetLife life = setting.GetAssetLife();
            if (life != null)
                AddAssetLife(life);
        }
        public static BundleDownloader DownLoadBundle(string bundleName) => setting.GetBundleDownloader(GetUrlFromBundleName(bundleName), bundleName);
        public static int GetWebRequestTimeout() => setting.GetWebRequestTimeout();
        public static int GetWebRequestRetryCount() => setting.GetWebRequestRetryCount();
        public static FileData.FileCompareType GetFileCheckType() => setting.GetFileCheckType();
        private static string GetUrlFromBundleName(string bundleName) => setting.GetUrlByBundleName(buildTarget, bundleName);
        public static IAssetStreamEncrypt GetEncrypt() => setting.GetEncrypt();
        private static bool GetAutoUnloadBundle() => setting.GetAutoUnloadBundle();
        public static bool GetSaveBundlesWhenPlaying() => setting.GetSaveBundlesWhenPlaying();
        public static long GetLoadingMaxTimeSlice() => setting.GetLoadingMaxTimeSlice();

    }
    partial class AssetsInternal
    {
        public static string RawToRawObjectPath(string path)
        {
            var dir = AssetsHelper.GetDirectoryName(path);
            var name = AssetsHelper.GetFileNameWithoutExtension(path);
            return AssetsHelper.ToRegularPath(AssetsHelper.CombinePath(dir, $"{name}.asset"));
        }
        public static string GetRawFileToDlcPath(string path)
        {
            string name = AssetsHelper.GetFileNameWithoutExtension(path);
            string ex = AssetsHelper.GetFileExtension(path);
            string hash = AssetsHelper.GetStringHash(name);
            return AssetsHelper.CombinePath(localRfcDir, $"{hash}{ex}");
        }


        public static Downloader DownloadVersion(string bundleName) => new Downloader(GetUrlFromBundleName(bundleName), GetWebRequestTimeout(), GetWebRequestRetryCount());





        public static string GetBundleLocalPath(string bundleName) => AssetsHelper.CombinePath(localSaveDir, bundleName);
        public static void UnloadBundles() => bundles.UnloadBundles();

        public static Bundle LoadBundle(string bundleName, bool async) => bundles.LoadAsync(new BundleLoadArgs(bundleName, async));





        public static Asset LoadFileAsset(string path, bool async) => assets.LoadAssetAsync(new AssetLoadArgs(path, true, path.EndsWith("unity"), null, "", async)) as Asset;
        public static AssetHandle LoadAsset(string path, bool async)
        {
            assets.RemoveUselessAsset();
            path = AssetsHelper.ToRegularPath(path);
            if (!ContainsAsset(path))
            {
                AssetsHelper.LogError($"Not Found Asset: {path}");
                return null;
            }
            AssetType type = GetAssetType(path);
            if (type == AssetType.Raw || type == AssetType.RawCopyFile)
                path = RawToRawObjectPath(path);

            List<AssetHandle> result = null;
            IReadOnlyList<string> dps = GetAssetDependencies(path);
            if (dps != null)
            {
                var find = assets.Find(path);
                if (find != null)
                    result = find.dps;
                else
                    result = new List<AssetHandle>();
                result.Clear();
                foreach (var item in dps)
                {
                    AssetHandle _asset = LoadAsset(item, async);
                    if (_asset != null)
                        result.Add(_asset);
                }
            }
            return assets.LoadAssetAsync(new AssetLoadArgs(path, false, path.EndsWith("unity"), result, "", async));
        }

        public static bool GetIsAssetLoaded(string assetPath) => assets.Find(assetPath) != null;



        public static void Release(string assetPath) => assets.Release(assetPath);



        public static IReadOnlyList<AssetHandle> GetLoadedAssets() => assets.GetAll();
        public static IReadOnlyList<Bundle> GetLoadedBundles() => bundles.GetAll();





    }
}

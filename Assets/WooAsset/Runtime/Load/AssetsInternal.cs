using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        public static string localSaveDir;


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
        static AssetsInternal()
        {
            mode = _defaultMode;
            bundles = new BundleMap();
            assets = new AssetMap();

            localSaveDir = CombinePath(Application.persistentDataPath, "DLC");
            if (!Directory.Exists(localSaveDir))
                Directory.CreateDirectory(localSaveDir);
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
        private static AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg)
        {
            if (!assetPath.StartsWith("Assets") || assetPath.Contains("Resources"))
            {
                if (assetPath.Contains("Resources"))
                {
                    var index = arg.path.LastIndexOf("Resources");
                    arg.path = arg.path.Remove(0, index + "Resources".Length + 1);
                }
                return new ResourcesAsset(arg);
            }
            return mode.CreateAsset(assetPath, arg);
        }
        public static IReadOnlyList<string> GetAllAssetPaths() => mode.GetAllAssetPaths();
        public static IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => mode.GetAssetsByAssetName(name, result);

        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => mode.GetTagAssetPaths(tag);
        public static IReadOnlyList<string> GetAllTags() => mode.GetAllTags();
        public static IReadOnlyList<string> GetAssetTags(string assetPath) => mode.GetAssetTags(assetPath);
        public static AssetType GetAssetType(string assetPath) => mode.GetAssetType(assetPath);
        private static IReadOnlyList<string> GetAssetDependencies(string assetPath) => mode.GetAssetDependencies(assetPath);
        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => mode.GetAllAssetPaths(bundleName);
        public static bool Initialized() => mode.Initialized();
        public static AssetOperation InitAsync(string version, bool again, string[] tags) => mode.InitAsync(version, again, tags);
        public static CheckBundleVersionOperation VersionCheck() => mode.VersionCheck();
        public static CopyBundleOperation CopyToSandBox() => mode.CopyToSandBox(CombinePath(Application.streamingAssetsPath, buildTarget), localSaveDir, false);

        public static BundleDownloader DownLoadBundle(string bundleName) => setting.GetBundleDownloader(GetUrlFromBundleName(bundleName), bundleName);
        public static Downloader DownloadVersion(string bundleName) => new Downloader(GetUrlFromBundleName(bundleName), GetWebRequestTimeout());
        public static void SetAssetsSetting(AssetsSetting setting)
        {
            AssetsInternal.setting = setting;
            IAssetLife life = setting.GetAssetLife();
            if (life != null)
                AddAssetLife(life);
        }
        public static int GetWebRequestTimeout() => setting.GetWebRequestTimeout();
        public static FileData.FileCompareType GetFileCheckType() => setting.GetFileCheckType();
        private static string GetUrlFromBundleName(string bundleName) => setting.GetUrlByBundleName(buildTarget, bundleName);
        public static IAssetStreamEncrypt GetEncrypt() => setting.GetEncrypt();
        private static bool GetAutoUnloadBundle() => setting.GetAutoUnloadBundle();
        public static bool GetSaveBundlesWhenPlaying() => setting.GetSaveBundlesWhenPlaying();




        public static string GetBundleLocalPath(string bundleName) => CombinePath(localSaveDir, bundleName);
        public static void UnloadBundles() => bundles.UnloadBundles();

        public static Bundle LoadBundle(string bundleName, bool async)
        {
            return bundles.LoadAsync(new BundleLoadArgs(bundleName, async));
        }

        private static void ReleaseBundleByAsset(AssetHandle asset)
        {
            if (asset.bundle == null) return;
            bundles.Release(asset.bundle.bundleName);
        }

        public static string RawToRawObjectPath(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var name = Path.GetFileNameWithoutExtension(path);
            return ToRegularPath(Path.Combine(dir, $"{name}.asset"));
        }


        public static AssetHandle LoadAsset(string path, bool async)
        {
            assets.RemoveUselessAsset();
            if (GetAssetType(path) == AssetType.Raw)
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
            return assets.LoadAssetAsync(new AssetLoadArgs(path, path.EndsWith("unity"), result, "", async));
        }

        public static bool GetIsAssetLoaded(string assetPath) => assets.Find(assetPath) != null;



        public static void Release(string assetPath) => assets.Release(assetPath);



        public static IReadOnlyList<AssetHandle> GetLoadedAssets() => assets.GetAll();
        public static IReadOnlyList<Bundle> GetLoadedBundles() => bundles.GetAll();



        public static void LogWarning(string msg) => Debug.LogWarning("Assets : " + msg);
        public static void Log(string msg) => Debug.Log("Assets : " + msg);
        public static void LogError(string err) => Debug.LogError("Assets : " + err);
        private static string ToHashString(byte[] bytes)
        {
            byte[] retVal = MD5.Create().ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetStringHash(string str) => ToHashString(Encoding.Default.GetBytes(str));
        public static string GetFileHash(string path) => File.Exists(path) ? ToHashString(File.ReadAllBytes(path)) : string.Empty;
        public static string CombinePath(string self, string combine) => Path.Combine(self, combine);
        public static string ToRegularPath(string path) => path.Replace('\\', '/');









    }
}

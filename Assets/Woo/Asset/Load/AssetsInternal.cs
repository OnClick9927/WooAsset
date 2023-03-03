

using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static WooAsset.AssetsSetting;

namespace WooAsset
{
    public static partial class AssetsInternal
    {
        private static BundleMap bundles;
        private static AssetMap assets;
        private static AssetsSetting setting;
        private static AssetManifest manifest;

        private static IAssetMode _defaultMode = new NormalAssetMode();
        public static bool isNormalMode => mode is NormalAssetMode;
        public static IAssetMode mode { get; set; }
        public static string localSaveDir;


        private static string buildTarget
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer: return "Windows";
                    case RuntimePlatform.Android: return "Android";
                    case RuntimePlatform.IPhonePlayer: return "iOS";
                    case RuntimePlatform.WebGLPlayer: return "WebGL";
                }
                return string.Empty;
            }
        }
        static AssetsInternal()
        {

            mode = _defaultMode;
            bundles = new BundleMap();
            assets = new AssetMap();
            localSaveDir = CombinePath(Application.persistentDataPath, "DLC");
        }

        public static void SetAssetListen(IAssetLife<Asset> asset, IAssetLife<Bundle> bundle)
        {
            bundles.SetListen(bundle);
            assets.SetListen(asset);
        }

    }
    partial class AssetsInternal
    {
        private static Asset CreateAsset(string assetPath, List<Asset> dps, AssetLoadArgs arg)
        {
            if (!assetPath.StartsWith("Assets") || assetPath.Contains("Resources"))
            {
                if (assetPath.Contains("Resources"))
                {
                    var idnex = arg.path.LastIndexOf("Resources");
                    arg.path = arg.path.Remove(0, idnex + "Resources".Length + 1);
                }
                return new ResourcesAsset(null, null, arg);
            }
            return mode.CreateAsset(assetPath, dps, arg);
        }
        private static SceneAsset CreateSceneAsset(string assetPath, List<Asset> dps, SceneAssetLoadArgs arg) => mode.CreateSceneAsset(assetPath, dps, arg);
        public static IReadOnlyList<string> GetAllAssetPaths() => mode.GetAllAssetPaths();
        public static IReadOnlyList<string> GetTagAssetPaths(string tag)
        {
            return mode.GetTagAssetPaths(tag);
        }
        public static IReadOnlyList<string> GetAllTags()
        {
            return mode.GetAllTags();
        }
        public static string GetAssetTag(string assetPath) => mode.GetAssetTag(assetPath);

        public static void SetAssetsSetting(AssetsSetting setting) => AssetsInternal.setting = setting;
        private static int GetWebRequestTimeout() => setting.GetWebRequestTimeout();
        private static FileCheckType GetFileCheckType() => setting.GetFileCheckType();
        private static string GetUrlFromBundleName(string bundleName) => setting.GetUrlByBundleName(buildTarget, bundleName);
        private static string GetVersionUrl() => setting.GetVersionUrl(buildTarget);
        private static IAssetStreamEncrypt GetEncrypt() => setting.GetEncrypt();
        private static bool GetAutoUnloadBundle() => setting.GetAutoUnloadBundle();




        private static string GetBundleLocalPath(string bundleName) => CombinePath(localSaveDir, bundleName);
        private static string[] GetLocalBundles()
        {
            var files = Directory.GetFiles(localSaveDir);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = AssetsInternal.ToRegularPath(files[i]);
            }
            return files;
        }
        public static void UnloadBundles() => bundles.UnloadBundles();

        private static bool IsManifestNull() => manifest == null;

        private static Bundle LoadBundleAsync(string bundleName)
        {
            string filePath = GetBundleLocalPath(bundleName);
            if (!File.Exists(filePath))
                return bundles.RequestLoadAsync(GetUrlFromBundleName(bundleName), bundleName);
            return bundles.LoadAsync(filePath, bundleName);
        }

        private static string GetBundleNameByAssetPath(string assetPath) => manifest.GetBundle(assetPath);
        private static void ReleaseBundleByAssetPath(string assetPath)
        {
            if (IsManifestNull()) return;
            string bundle = GetBundleNameByAssetPath(assetPath);
            bundles.Release(bundle);
        }
        private static Bundle LoadBundleByAssetPath(string assetPath)
        {
            string bundleName = GetBundleNameByAssetPath(assetPath);
            if (string.IsNullOrEmpty(bundleName))
            {
                LogError($"can not find asset {assetPath}");
            }
            return LoadBundleAsync(bundleName);
        }




        public static Asset LoadAssetAsync(string path)
        {
            LoadDps(path);
            Asset asset = assets.LoadAssetAsync(path);
            return asset;
        }
        public static SceneAsset LoadSceneAssetAsync(string path)
        {
            LoadDps(path);
            SceneAsset asset = assets.LoadSceneAssetAsync(path);
            return asset;
        }
        private static List<string> GetAssetDps(string assetPath)
        {
            return IsManifestNull() ? null : manifest.GetAssetDependencies(assetPath);
        }

        private static void LoadDps(string path)
        {
            List<string> dps = GetAssetDps(path);
            if (dps != null)
            {
                foreach (var item in dps)
                {
                    LoadAssetAsync(item);
                }
            }
        }
        public static void Release(Asset asset)
        {
            assets.Release(asset.path);
        }


        public static void LogError(string err)
        {
            Debug.LogError("Assets : " + err);
        }

        public static string GetNameHash(string str)
        {
            MD5CryptoServiceProvider md5CSP = new MD5CryptoServiceProvider();
            byte[] retVal = md5CSP.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetFileHash(string path)
        {
            if (!File.Exists(path))
                return "";

            MD5CryptoServiceProvider md5CSP = new MD5CryptoServiceProvider();
            FileStream file = new FileStream(path, FileMode.Open);
            byte[] retVal = md5CSP.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }


        public static CopyBundleOperation CopyDLCFromSteam()
        {
            return CopyDirectory(CombinePath(Application.streamingAssetsPath, buildTarget), localSaveDir);
        }


        public static CopyBundleOperation CopyDirectory(string srcPath, string destPath)
        {
            return new CopyBundleOperation(srcPath, destPath);
        }

        public static string CombinePath(string self, string combine)
        {
            return Path.Combine(self, combine);
        }
        public static string ToRegularPath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}

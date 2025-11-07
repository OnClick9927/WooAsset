using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WooAsset
{
    public partial class Assets
    {

        public static void SetLocalSaveDir(string path) => AssetsInternal.SetLocalSaveDir(path);
        public static void SetAssetsSetting(AssetsSetting setting) => AssetsInternal.SetAssetsSetting(setting);
        public static LoadRemoteVersionsOperation LoadRemoteVersions() => AssetsInternal.LoadRemoteVersions();
        public static VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType = VersionCompareType.Manifest) => AssetsInternal.CompareVersion(version, pkgs, compareType);
        public static LoadVersionDataOperation DownloadVersionData(string version) => AssetsInternal.DownloadVersionData(version);

        public static DownLoader DownLoadBundleFile(string version, string bundleName) => AssetsInternal.DownLoadBundleFile(version, bundleName);
        public static Operation CopyToSandBox() => AssetsInternal.CopyToSandBox();
        public static bool Initialized() => AssetsInternal.Initialized();
        public static Operation InitAsync(string version = "", bool ignoreLocalVersion = false, bool again = false, Func<VersionData, List<PackageData>> getPkgs = null) => AssetsInternal.InitAsync(version, ignoreLocalVersion, again, getPkgs);
        public static Asset LoadAssetAsync(string path, Type type) => AssetsInternal.LoadAsset(path, false, true, type) as Asset;
        public static Asset LoadAsset(string path, Type type) => AssetsInternal.LoadAsset(path, false, false, type) as Asset;
        public static Asset LoadAssetAsync(string path) => LoadAssetAsync(path, typeof(UnityEngine.Object));
        public static Asset LoadAsset(string path) => LoadAsset(path, typeof(UnityEngine.Object));
        public static Asset LoadAssetAsync<T>(string path) where T : UnityEngine.Object => LoadAssetAsync(path, typeof(T));
        public static Asset LoadAsset<T>(string path) where T : UnityEngine.Object => LoadAsset(path, typeof(T));



        public static SubAsset LoadSubAssetAsync(string path, Type type) => AssetsInternal.LoadAsset(path, true, true, type) as SubAsset;
        public static SubAsset LoadSubAsset(string path, Type type) => AssetsInternal.LoadAsset(path, true, false, type) as SubAsset;
        public static SubAsset LoadSubAssetAsync(string path) => LoadSubAssetAsync(path, typeof(UnityEngine.Object));
        public static SubAsset LoadSubAsset(string path) => LoadSubAsset(path, typeof(UnityEngine.Object));
        public static SubAsset LoadSubAssetAsync<T>(string path) where T : UnityEngine.Object => LoadSubAssetAsync(path, typeof(T));
        public static SubAsset LoadSubAsset<T>(string path) where T : UnityEngine.Object => LoadSubAsset(path, typeof(T));




        public static RawAsset LoadRawAssetAsync(string path) => AssetsInternal.LoadAsset(path, false, true, null) as RawAsset;
        public static RawAsset LoadRawAsset(string path) => AssetsInternal.LoadAsset(path, false, false, null) as RawAsset;
        public static SceneAsset LoadSceneAssetAsync(string path) => AssetsInternal.LoadAsset(path, false, true, null) as SceneAsset;
        public static SceneAsset LoadSceneAsset(string path) => AssetsInternal.LoadAsset(path, false, false, null) as SceneAsset;
        public static AsyncOperation UnloadSceneAsync(string path, UnloadSceneOptions op) => AssetsInternal.UnloadSceneAsync(path, op);




        public static void Release(AssetHandle asset) => AssetsInternal.Release(asset.path);
        public static void Release(string assetPath) => AssetsInternal.Release(assetPath);

        public static void UnloadBundles() => AssetsInternal.UnloadBundles();
        public static bool GetIsAssetLoaded(string assetPath) => AssetsInternal.GetIsAssetLoaded(assetPath);



        public static IReadOnlyList<string> GetAssetTags(string assetPath) => AssetsInternal.GetAssetTags(assetPath);
        public static IReadOnlyList<string> GetAllAssetPaths() => AssetsInternal.GetAllAssetPaths();
        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => AssetsInternal.GetTagAssetPaths(tag);
        public static IReadOnlyList<string> GetAllTags() => AssetsInternal.GetAllTags();
        public static string GetUniqueAssetPathByTag(string tag)
        {
            var assets = Assets.GetTagAssetPaths(tag);
            if (assets == null || assets.Count != 1)
                return string.Empty;
            return assets[0];
        }
        public static IReadOnlyList<string> GetAssetPath(Func<AssetData, bool> fit)
        {
            var assets = Assets.GetAllAssetPaths();
            if (fit == null)
                return assets;
            if (assets == null || assets.Count == 0)
                return null;
            return assets.Where(x => fit(Assets.GetAssetData(x))).ToList();
        }
        public static string GetUniqueAssetPath(Func<AssetData, bool> fit)
        {
            var assets = Assets.GetAllAssetPaths();
            if (assets == null || assets.Count == 0)
                return string.Empty;
            if (fit == null)
                return assets[0];
            for (int i = 0; i < assets.Count; i++)
            {
                var data = Assets.GetAssetData(assets[i]);
                if (fit(data)) return assets[i];
            }
            return string.Empty;
        }
        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => AssetsInternal.GetAllAssetPaths(bundleName);
        public static IReadOnlyList<string> GetAssetsByAssetName(string name) => AssetsInternal.GetAssetsByAssetName(name);

        public static AssetData GetAssetData(string assetPath) => AssetsInternal.GetAssetData(assetPath);
        public static string GetRawAssetLocalPath(string assetPath)
        {
            var data = GetAssetData(assetPath);
            return AssetsInternal.GetBundleLocalPath(data.bundleName);
        }
        public static string GetRawAssetRemoteUrl(string assetPath)
        {
            var data = GetAssetData(assetPath);
            return AssetsInternal.GetUrlFromBundleName(AssetsInternal.GetVersion(), data.bundleName);
        }
        public static string GetRawAssetUrlOrPath(string assetPath)
        {
            if (AssetsInternal.isNormalMode)
            {
                var data = GetAssetData(assetPath);
                var type = Bundle.GetLoadType(data.bundleName);
                if (type == Bundle.BundleLoadType.FromFile)
                    return AssetsInternal.GetBundleLocalPath(data.bundleName);
                return AssetsInternal.GetUrlFromBundleName(AssetsInternal.GetVersion(), data.bundleName);
            }
            return assetPath;

        }
    }

    partial class Assets
    {

        static Dictionary<string, WooAsset.AssetCollection> assetCollections = new Dictionary<string, AssetCollection>();
        public static IAssetCollection FindAssetCollection(string key)
        {
            WooAsset.AssetCollection collection = null;
            assetCollections.TryGetValue(key, out collection);
            return collection;
        }
        public static IAssetCollection GetAssetCollection(string key)
        {
            WooAsset.AssetCollection collection = null;
            if (!assetCollections.TryGetValue(key, out collection))
            {
                collection = AssetCollection.Get();
                assetCollections.Add(key, collection);
            }

            return collection;
        }
        public static void ClearAssetCollection(string key)
        {
            AssetCollection find;
            if (assetCollections.Remove(key, out find))
            {
                find.Clear();
                AssetCollection.Set(find);
            }
        }
        public static void ClearAllAssetCollection()
        {
            foreach (var item in assetCollections.Values)
            {
                item.Clear();
                AssetCollection.Set(item);
            }
            assetCollections.Clear();
        }








        private static List<IAssetBridge> m_Assets = new List<IAssetBridge>();



        public static void ReleaseUselessBridges()
        {
            m_Assets.RemoveAll(x =>
            {
                var could = x.CouldRelease();
                if (could)
                    x.Release();
                return could;
            });

        }
        public static void AddBridge<T>(AssetBridge<T> asset) where T : class
        {
            m_Assets.Add(asset);
        }
        public static void ReleaseBridge<T>(T context) where T : class
        {
            var find = m_Assets.Find(x =>
            {
                if ((x as AssetBridge<T>).context == context)
                {
                    var could = x.CouldRelease();
                    return could;
                }
                return false;
            });
            if (find == null) return;
            m_Assets.Remove(find);
            find.Release();
        }


        public static AssetsGroupOperation PrepareAssets(IReadOnlyList<string> paths) => new AssetsGroupOperation(paths);
        public static AssetsGroupOperation PrepareAssetsByTag(string tag) => PrepareAssets(Assets.GetTagAssetPaths(tag));

        public static InstantiateObjectOperation InstantiateAsync(Asset asset, Transform parent) => new InstantiateObjectOperation(asset, parent);
        public static InstantiateObjectOperation InstantiateAsync(string path, Transform parent) => InstantiateAsync(Assets.LoadAssetAsync<GameObject>(path), parent);
        public static void Destroy(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                GameObject.Destroy(gameObject);
            else
                GameObject.DestroyImmediate(gameObject);
#else
            GameObject.Destroy(gameObject);
#endif
            Assets.ReleaseBridge(gameObject);
        }
    }

}

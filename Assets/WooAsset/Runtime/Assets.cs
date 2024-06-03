using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WooAsset
{
    public partial class Assets
    {
        public static void SetAssetsSetting(AssetsSetting setting) => AssetsInternal.SetAssetsSetting(setting);
        public static CheckBundleVersionOperation LoadRemoteVersions() => AssetsInternal.LoadRemoteVersions();
        public static VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType = VersionCompareType.Manifest) => AssetsInternal.CompareVersion(version, pkgs, compareType);
        public static LoadVersionDataOperation DonloadVersionData(string version) => AssetsInternal.DonloadVersionData(version);

        public static FileDownloader DownLoadBundle(string version, string bundleName) => AssetsInternal.DownLoadFile(version, bundleName);
        public static Operation CopyToSandBox() => AssetsInternal.CopyToSandBox();
        public static bool Initialized() => AssetsInternal.Initialized();
        public static Operation InitAsync(string version = "", bool again = false, Func<VersionData, List<PackageData>> getPkgs = null) => AssetsInternal.InitAsync(version, again, getPkgs);
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

        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => AssetsInternal.GetAllAssetPaths(bundleName);

    }

    partial class Assets
    {
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


        public static AssetsGroupOperation PrepareAssets(string[] paths) => new AssetsGroupOperation(paths);
        public static AssetsGroupOperation PrepareAssetsByTag(string tag) => new AssetsGroupOperation(Assets.GetTagAssetPaths(tag)?.ToArray());
        public static InstantiateObjectOperation InstantiateAsync(string path, Transform parent) => new InstantiateObjectOperation(path, parent);
        public static void Destroy(GameObject gameObject)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(gameObject);
#else
            GameObject.Destroy(gameObject);
#endif
            Assets.ReleaseBridge(gameObject);
        }
    }

    partial class Assets
    {
        public class Search
        {

            private static IEnumerable<string> _ITags(string[] tags)
            {
                IEnumerable<string> result = null;
                for (int i = 0; i < tags.Length; i++)
                {
                    var tmp = AssetsInternal.GetTagAssetPaths(tags[i]);
                    if (result == null)
                        result = tmp;
                    else
                        result = result.Intersect(tmp);
                }
                if (result == null) return new List<string>();
                return result;
            }
            public static IEnumerable<string> ITags(params string[] tags)
            {
                return _ITags(tags);
            }
            private static List<string> _UTags(string[] tags, List<string> result)
            {
                result.Clear();
                if (tags == null || tags.Length == 0) return result;
                for (int i = 0; i < tags.Length; i++)
                {
                    var assets = AssetsInternal.GetTagAssetPaths(tags[i]);
                    for (int j = 0; j < assets.Count; j++)
                    {
                        if (result.Contains(assets[j])) continue;
                        result.Add(assets[j]);
                    }
                }
                return result;
            }
            public static IReadOnlyList<string> UTags(params string[] tags)
            {
                return _UTags(tags, new List<string>());
            }
            public static IReadOnlyList<string> UNames(params string[] names)
            {
                List<string> result = new List<string>();
                if (names == null || names.Length == 0) return result;
                List<string> tmp = new List<string>();

                for (int i = 0; i < names.Length; i++)
                {
                    var assets_2 = AssetsInternal.GetAssetsByAssetName(names[i], tmp);
                    for (int j = 0; j < assets_2.Count; j++)
                    {
                        if (result.Contains(assets_2[j])) continue;
                        result.Add(assets_2[j]);
                    }
                }
                return result;
            }
            public static IReadOnlyList<string> UNamesUTags(params string[] nameOrTags)
            {
                List<string> result = new List<string>();
                var r_1 = UNames(nameOrTags);
                var r_2 = UTags(nameOrTags);
                foreach (var item in r_1)
                {
                    if (result.Contains(item)) continue;
                    result.Add(item);
                }
                foreach (var item in r_2)
                {
                    if (result.Contains(item)) continue;
                    result.Add(item);
                }
                return result;
            }



            public static IEnumerable<string> INameITags(string assetName, params string[] tags)
            {
                var result = _ITags(tags);
                return result.Where(x => AssetsHelper.GetFileName(x).Contains(assetName));
            }
            public static IEnumerable<string> ITypeITags(AssetType type, params string[] tags)
            {
                var result = _ITags(tags);
                return result.Where(x => AssetsInternal.GetAssetType(x) == type);
            }
            public static IEnumerable<string> ITypeINameITags(AssetType type, string assetName, params string[] tags)
            {
                return INameITags(assetName, tags).Where(x => AssetsInternal.GetAssetType(x) == type);
            }

            public static IEnumerable<string> INameUTags(string assetName, params string[] tags)
            {
                var result = _UTags(tags, new List<string>());
                return result.Where(x => AssetsHelper.GetFileName(x).Contains(assetName)); ;
            }
            public static IEnumerable<string> ITypeUTags(AssetType type, params string[] tags)
            {
                var result = _UTags(tags, new List<string>());
                return result.Where(x => AssetsInternal.GetAssetType(x) == type);
            }
            public static IEnumerable<string> ITypeINameUTags(AssetType type, string assetName, params string[] tags)
            {
                return INameUTags(assetName, tags).Where(x => AssetsInternal.GetAssetType(x) == type);
            }
            public static IEnumerable<string> ITypeUNameUTags(AssetType type, params string[] nameOrTags)
            {
                return UNamesUTags(nameOrTags).Where(x => AssetsInternal.GetAssetType(x) == type);
            }


            public static IReadOnlyList<string> AssetPathByType(AssetType type)
            {
                IReadOnlyList<string> assets = AssetsInternal.GetAllAssetPaths();
                return assets.Where(x => AssetsInternal.GetAssetType(x) == type).ToList();
            }

        }
    }
}

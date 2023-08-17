using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WooAsset
{
    public partial class Assets
    {
        public static void SetAssetsSetting(AssetsSetting setting) => AssetsInternal.SetAssetsSetting(setting);
        public static CheckBundleVersionOperation VersionCheck() => AssetsInternal.VersionCheck();
        public static DownLoadBundleOperation DownLoadBundle(string bundleName) => new DownLoadBundleOperation(bundleName);
        public static CopyDirectoryOperation CopyToSandBox() => AssetsInternal.CopyToSandBox();
        public static bool Initialized() => AssetsInternal.Initialized();
        public static Operation InitAsync(string version = "", bool again = false, params string[] tags) => AssetsInternal.InitAsync(version, again, tags);
        public static UnzipRawFileOperation UnzipRawFile() => AssetsInternal.UnzipRawFile();
        public static Asset LoadAssetAsync(string path) => AssetsInternal.LoadAsset(path, true) as Asset;
        public static Asset LoadAsset(string path) => AssetsInternal.LoadAsset(path, false) as Asset;
        public static SceneAsset LoadSceneAssetAsync(string path) => AssetsInternal.LoadAsset(path, true) as SceneAsset;
        public static SceneAsset LoadSceneAsset(string path) => AssetsInternal.LoadAsset(path, false) as SceneAsset;
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
        public static AssetsGroupOperation PrepareAssetsByTag(string tag) => new AssetsGroupOperation(Assets.GetTagAssetPaths(tag).ToArray());
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

            private static List<string> _IntersectTag(string[] tags, List<string> searchList)
            {
                searchList.Clear();
                IReadOnlyList<string> assets = AssetsInternal.GetAllAssetPaths();
                for (int i = 0; i < assets.Count; i++)
                {
                    var assetPath = assets[i];
                    var assetTags = AssetsInternal.GetAssetTags(assetPath);
                    bool add = true;
                    if (tags != null)
                    {
                        for (int j = 0; j < tags.Length; j++)
                            if (!assetTags.Contains(tags[j]))
                                add = false;
                    }
                    if (!add) continue;
                    searchList.Add(assetPath);
                }
                return searchList;
            }
            public static IReadOnlyList<string> IntersectNameAndTag(string assetName, params string[] tags)
            {
                var searchList = _IntersectTag(tags, new List<string>());
                searchList.RemoveAll(x => !AssetsHelper.GetFileName(x).Contains(assetName));
                return searchList;
            }
            public static IReadOnlyList<string> IntersectTag(params string[] tags)
            {
                return _IntersectTag(tags, new List<string>());
            }

            public static IReadOnlyList<string> IntersectTypeAndNameAndTag(AssetType type, string assetName, params string[] tags)
            {
                var searchList = _IntersectTag(tags, new List<string>());
                searchList.RemoveAll(x => !AssetsHelper.GetFileName(x).Contains(assetName));
                searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
                return searchList;
            }
            public static IReadOnlyList<string> IntersectTypeAndTag(AssetType type, params string[] tags)
            {
                var searchList = _IntersectTag(tags, new List<string>());
                searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
                return searchList;
            }



            private static List<string> _UnionTag(string[] tags, List<string> searchList)
            {
                searchList.Clear();
                if (tags == null) return searchList;
                for (int i = 0; i < tags.Length; i++)
                {
                    var assets = AssetsInternal.GetTagAssetPaths(tags[i]);
                    searchList.AddRange(assets);
                }
                return searchList.Distinct().ToList();
            }
            public static IReadOnlyList<string> Union(params string[] nameOrTags)
            {
                List<string> searchList = new List<string>();
                List<string> searchList2 = new List<string>();

                searchList.Clear();
                if (nameOrTags == null) return searchList;
                for (int i = 0; i < nameOrTags.Length; i++)
                {
                    var assets = AssetsInternal.GetTagAssetPaths(nameOrTags[i]);
                    var assets_2 = AssetsInternal.GetAssetsByAssetName(nameOrTags[i], searchList2);
                    searchList.AddRange(assets_2);
                    searchList.AddRange(assets);
                }
                return searchList.Distinct().ToList();
            }
            public static IReadOnlyList<string> UnionTag(params string[] tags)
            {
                return _UnionTag(tags, new List<string>());
            }
            public static IReadOnlyList<string> UnionName(params string[] names)
            {
                List<string> searchList = new List<string>();
                List<string> searchList2 = new List<string>();

                searchList.Clear();
                if (names == null) return searchList;
                for (int i = 0; i < names.Length; i++)
                {
                    var assets_2 = AssetsInternal.GetAssetsByAssetName(names[i], searchList2);
                    searchList.AddRange(assets_2);
                }
                return searchList.Distinct().ToList();
            }
            public static IReadOnlyList<string> UnionNameAndTag(string assetName, params string[] tags)
            {
                var searchList = _UnionTag(tags, new List<string>());
                searchList.RemoveAll(x => !AssetsHelper.GetFileName(x).Contains(assetName));
                return searchList;
            }
            public static IReadOnlyList<string> UnionTypeAndNameAndTag(AssetType type, string assetName, params string[] tags)
            {
                var searchList = _UnionTag(tags, new List<string>());
                searchList.RemoveAll(x => !AssetsHelper.GetFileName(x).Contains(assetName));
                searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
                return searchList;
            }
            public static IReadOnlyList<string> UnionTypeAndTag(AssetType type, params string[] tags)
            {
                var searchList = _UnionTag(tags, new List<string>());
                searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
                return searchList;
            }


            public static IReadOnlyList<string> AssetPathByType(AssetType type)
            {
                IReadOnlyList<string> assets = AssetsInternal.GetAllAssetPaths();
                return assets.Where(x => AssetsInternal.GetAssetType(x) == type).ToList();
            }

        }
    }
}

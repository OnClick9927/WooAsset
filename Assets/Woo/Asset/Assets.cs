

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static WooAsset.AssetsInternal;

namespace WooAsset
{
    public partial class Assets
    {

        private static LoadManifestOperation manifestOp;
        private static bool IsScene(string path)
        {
            return path.EndsWith("unity");
        }


        public static void SetAssetsSetting(AssetsSetting setting)
        {
            AssetsInternal.SetAssetsSetting(setting);
        }
        public static CheckBundleVersionOperation VersionCheck()
        {
            return new CheckBundleVersionOperation();
        }
        public static DownLoadBundleOperation DownLoadBundle(string bundleName)
        {
            return new DownLoadBundleOperation(bundleName);
        }
        public static CopyBundleOperation CopyDLCFromSteam()
        {
            return AssetsInternal.CopyDLCFromSteam();
        }
        public static bool Initialized()
        {
            if (manifestOp == null) return false;
            return manifestOp.isDone;
        }
        public static LoadManifestOperation InitAsync(bool again = false)
        {
            if (again)
                manifestOp = null;
            if (manifestOp == null)
                manifestOp = new LoadManifestOperation();
            return manifestOp;
        }
        public static Asset LoadAssetAsync(string path)
        {
            if (IsScene(path))
            {
                return LoadSceneAssetAsync(path);
            }
            return AssetsInternal.LoadAssetAsync(path);
        }
        public static SceneAsset LoadSceneAssetAsync(string path)
        {
            return AssetsInternal.LoadSceneAssetAsync(path);
        }
        public static void Release(Asset asset)
        {
            AssetsInternal.Release(asset);
        }
        public static void UnloadBundles() => AssetsInternal.UnloadBundles();



        public static IReadOnlyList<string> GetAllAssetPaths()
        {
            return AssetsInternal.GetAllAssetPaths();
        }
        public static IReadOnlyList<string> GetTagAssetPaths(string tag)
        {
            return AssetsInternal.GetTagAssetPaths(tag);
        }
        public static IReadOnlyList<string> GetAllTags()
        {
            return AssetsInternal.GetAllTags();
        }
        public static AssetsGroupOperation PrepareAssets(string[] paths)
        {
            return new AssetsGroupOperation(paths);
        }
        public static AssetsGroupOperation PrepareAssetsByTag(string tag)
        {
            var assets = GetTagAssetPaths(tag);
            return new AssetsGroupOperation(assets.ToArray());
        }
        public static InstantiateObjectOperation InstantiateAsync(string path, Transform parent)
        {
            return new InstantiateObjectOperation(path, parent);
        }
        public static void Destroy(GameObject gameObject)
        {
            InstantiateObjectOperation.Destroy(gameObject);
        }

    }
}

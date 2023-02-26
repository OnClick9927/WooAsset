

using System.Collections.Generic;
using UnityEngine;
using static WooAsset.AssetsInternal;

namespace WooAsset
{
    public partial class Assets
    {

        private static LoadManifestOperation manifestop;
        private static bool IsScene(string path)
        {
            return path.EndsWith("unity");
        }

        public static IReadOnlyList<string> GetAllAssetPaths()
        {
            return AssetsInternal.GetAllAssetPaths();
        }
        private static IReadOnlyList<string> GetTagAssetPaths(string tag)
        {
            return AssetsInternal.GetTagAssetPaths(tag);
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
            if (manifestop == null) return false;
            return manifestop.isDone;
        }
        public static LoadManifestOperation InitAsync(bool again = false)
        {
            if (again)
                manifestop = null;
            if (manifestop == null)
                manifestop = new LoadManifestOperation();
            return manifestop;
        }

        public static void UnloadBundles() => AssetsInternal.UnloadBundles();

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
        public static AssetsGroupOperation PrepareAssets(string[] paths)
        {
            return new AssetsGroupOperation(paths);
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

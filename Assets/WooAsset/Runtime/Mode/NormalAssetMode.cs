

using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{

    public partial class NormalAssetMode : IAssetMode
    {

        private LoadManifestOperation manifestOp;
        bool IAssetMode.Initialized()
        {
            if (manifestOp == null) return false;
            return manifestOp.isDone;
        }
        Operation IAssetMode.InitAsync(string version, bool again, string[] tags)
        {
            if (again)
            {
                manifestOp = null;
                AssetsInternal.UnloadBundles();
                var all = AssetsInternal.GetLoadedBundles();
                AssetsHelper.Log($"Still exist {all.Count} bundles when init again");
            }
            if (manifestOp == null)
                manifestOp = new LoadManifestOperation(AssetsInternal.GetLoadedBundles()
                    .Select(x => x.bundleName).ToList()
                    , version, tags);
            return manifestOp;
        }

        UnzipRawFileOperation IAssetMode.UnzipRawFile() => new UnzipRawFileOperation(manifestOp.manifest.rawAssets_copy);

        CheckBundleVersionOperation IAssetMode.VersionCheck() => new CheckBundleVersionOperation();

        AssetHandle IAssetMode.CreateAsset(string assetPath, AssetLoadArgs arg)
        {
            if (!arg.direct)
            {
                string bundleName = manifestOp.manifest.GetBundle(assetPath);
                if (string.IsNullOrEmpty(bundleName))
                    AssetsHelper.LogError($"Not Found  {assetPath}");
                arg.bundleName = bundleName;
            }
            if (arg.scene)
                return new SceneAsset(arg);
            return new Asset(arg);
        }

        IReadOnlyList<string> IAssetMode.GetAllAssetPaths()
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifestOp.manifest.GetAssets();
        }


        IReadOnlyList<string> IAssetMode.GetTagAssetPaths(string tag)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifestOp.manifest.GetTagAssetPaths(tag);
        }


        IReadOnlyList<string> IAssetMode.GetAssetTags(string assetPath)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifestOp.manifest.GetAssetTags(assetPath);
        }
        AssetType IAssetMode.GetAssetType(string assetPath)
        {
            if (!((IAssetMode)this).Initialized())
                return AssetType.None;
            return manifestOp.manifest.GetAssetType(assetPath);
        }


        IReadOnlyList<string> IAssetMode.GetAllTags()
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifestOp.manifest.GetAllTags();
        }

        IReadOnlyList<string> IAssetMode.GetAssetDependencies(string assetPath)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifestOp.manifest.GetAssetDependencies(assetPath);
        }

        IReadOnlyList<string> IAssetMode.GetAllAssetPaths(string bundleName)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifestOp.manifest.GetAssets(bundleName);
        }

        IReadOnlyList<string> IAssetMode.GetAssetsByAssetName(string name, List<string> result)
        {
            if (!((IAssetMode)this).Initialized())
                return null;
            return manifestOp.manifest.GetAssetsByAssetName(name, result);
        }



        CopyStreamBundlesOperation IAssetMode.CopyToSandBox(string from, string to) => new CopyStreamBundlesOperation(from, to);

        bool IAssetMode.ContainsAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            if (!((IAssetMode)this).Initialized())
                return false;
            return manifestOp.manifest.ContainsAsset(assetPath);

        }


    }

}

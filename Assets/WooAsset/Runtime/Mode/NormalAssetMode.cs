using System.Collections.Generic;
using System;
using System.Linq;
using static WooAsset.AssetsVersionCollection.VersionData;
using static WooAsset.AssetsVersionCollection;

namespace WooAsset
{

    public partial class NormalAssetMode : AssetMode
    {

        private LoadManifestOperation manifestOp;

        public override ManifestData manifest => Initialized() ? manifestOp.manifest : null;

        protected override bool Initialized()
        {
            if (manifestOp == null) return false;
            return manifestOp.isDone;
        }

        protected override CopyStreamBundlesOperation CopyToSandBox(string from, string to) => new CopyStreamBundlesOperation(from, to);

        protected override AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg)
        {
            if (!arg.direct)
            {
                string bundleName = AssetsInternal.GetAssetBundleName(assetPath);
                if (string.IsNullOrEmpty(bundleName))
                    AssetsHelper.LogError($"Not Found  {assetPath}");
                arg.bundleName = bundleName;
            }
            if (arg.scene)
                return new SceneAsset(arg);
            return new Asset(arg);
        }

        protected override Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs)
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
                    , version, getPkgs,AssetsInternal.GetEncrypt());
            return manifestOp;
        }

        protected override CheckBundleVersionOperation VersionCheck() => new CheckBundleVersionOperation();
    }

}

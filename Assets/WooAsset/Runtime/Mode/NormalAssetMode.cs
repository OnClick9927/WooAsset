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
                    , version, getPkgs, AssetsInternal.GetEncrypt());
            manifestOp.completed += ManifestOp_completed;
            return manifestOp;
        }

        private void ManifestOp_completed()
        {
            SetVersion(manifestOp.GetVersion());
        }

        protected override CheckBundleVersionOperation VersionCheck() => new CheckBundleVersionOperation();

        protected override Bundle CreateBundle(string bundleName, BundleLoadArgs args)
        {
            return new Bundle(args);
        }
    }

}

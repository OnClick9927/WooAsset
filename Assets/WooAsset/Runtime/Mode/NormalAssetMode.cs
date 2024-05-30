using System.Collections.Generic;
using System;
using System.Linq;

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
                AssetsHelper.Log($"Still exist {AssetsInternal.GetLoadedBundleCount()} bundles when init again");
            }
            if (manifestOp == null)
                manifestOp = new LoadManifestOperation(AssetsInternal.GetLoadedBundleNames().ToList()
                    , version, getPkgs);
            manifestOp.completed += ManifestOp_completed;
            return manifestOp;
        }

        private void ManifestOp_completed()
        {
            SetVersion(manifestOp.GetVersion());
        }

        protected override CheckBundleVersionOperation LoadRemoteVersions() => new CheckBundleVersionOperation();

        protected override Bundle CreateBundle(string bundleName, BundleLoadArgs args)
        {
            return new Bundle(args);
        }

        protected override VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs)
        {
           return new VersionCompareOperation(version, pkgs);
        }
    }

}

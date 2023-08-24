using System.Linq;

namespace WooAsset
{

    public partial class NormalAssetMode : AssetMode
    {

        private LoadManifestOperation manifestOp;

        protected override ManifestData manifest => Initialized() ? manifestOp.manifest : null;

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
                string bundleName = manifestOp.manifest.GetBundle(assetPath);
                if (string.IsNullOrEmpty(bundleName))
                    AssetsHelper.LogError($"Not Found  {assetPath}");
                arg.bundleName = bundleName;
            }
            if (arg.scene)
                return new SceneAsset(arg);
            return new Asset(arg);
        }

        protected override Operation InitAsync(string version, bool again, string[] tags)
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

        protected override CheckBundleVersionOperation VersionCheck() => new CheckBundleVersionOperation();
    }

}

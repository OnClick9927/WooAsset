using System.Collections.Generic;
using static WooAsset.ManifestData;

namespace WooAsset
{
    public class FastModeManifestTask : AssetTask
    {
        public static ManifestData BuildManifest(List<EditorBundleData> groups, AssetsTree tree)
        {
            List<AssetData> _assets = new List<AssetData>();
            List<BundleData> _bundles = new List<BundleData>();

            foreach (var build in groups)
            {
                _bundles.Add(build.CreateBundleData());
                foreach (var assetPath in build.GetAssets())
                {
                    EditorAssetData data = tree.GetAssetData(assetPath);
                    _assets.Add(data.CreateAssetData(build.hash));
                }
            }
            ManifestData manifest = new ManifestData();
            manifest.Read(_assets, _bundles);
            manifest.Prepare();
            return manifest;
        }
        protected override void OnExecute(AssetTaskContext context)
        {

            context.manifest = BuildManifest(context.allBundleBuilds, context.tree);
            InvokeComplete();
        }
    }
}

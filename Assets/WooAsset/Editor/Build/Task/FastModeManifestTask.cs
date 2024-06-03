using System.Collections.Generic;

namespace WooAsset
{
    public class FastModeManifestTask : AssetTask
    {
        public static ManifestData BuildManifest(string version, List<EditorBundleData> groups, AssetCollection tree, IAssetBuild assetBuild)
        {
            List<AssetData> _assets = new List<AssetData>();
            List<BundleData> _bundles = new List<BundleData>();

            foreach (var build in groups)
            {
                _bundles.Add(build.CreateBundleData());
                foreach (var assetPath in build.GetAssets())
                {
                    EditorAssetData data = tree.GetAssetData(assetPath);
                    if (assetBuild.NeedRecordAsset(data))
                        _assets.Add(data.CreateAssetData(build.hash));
                }
            }
            ManifestData manifest = new ManifestData();
            manifest.Read(version, _assets, _bundles);
            manifest.Prepare();
            return manifest;
        }
        protected override void OnExecute(AssetTaskContext context)
        {

            context.manifest = BuildManifest(context.version, context.allBundleBuilds, context.assetsCollection, context.assetBuild);
            InvokeComplete();
        }
    }
}

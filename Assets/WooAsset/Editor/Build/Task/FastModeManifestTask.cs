using System.Collections.Generic;
using static WooAsset.ManifestData;

namespace WooAsset
{
    public class FastModeManifestTask : AssetTask
    {
        public static ManifestData BuildManifest(List<BundleGroup> groups, AssetsTree tree)
        {
            List<AssetData> _assets = new List<AssetData>();
            foreach (var build in groups)
            {
                foreach (var assetPath in build.GetAssets())
                {
                    EditorAssetData data = tree.GetAssetData(assetPath);
                    _assets.Add(data.CreateAssetData(build.hash));
                }
            }
            ManifestData manifest = new ManifestData();
            manifest.Read(_assets, tree.GetRawAssets(), tree.GetRawAssets_Copy());
            manifest.Prepare();
            return manifest;
        }

        protected override void OnExecute(AssetTaskContext context)
        {
            context.manifest = BuildManifest(context.allBundleGroups,context.tree);
            InvokeComplete();
        }
    }
}

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
            manifest.Read(_assets);
            manifest.Prepare();
            return manifest;
        }
        protected override void OnExecute(AssetTaskContext context)
        {
            //var source = context.allBundleGroups;
            //if (source.Count != 0)
            //{
            //    AssetBundleManifest _main = BuildPipeline.BuildAssetBundles(context.historyPath,
            //         source.ConvertAll(x => x.ToAssetBundleBuild()).ToArray(), context.BuildOption | BuildAssetBundleOptions.DryRunBuild, context.buildTarget);
            //    UpdateHash(source, _main);
            //}
            context.manifest = BuildManifest(context.allBundleGroups, context.tree);
            InvokeComplete();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;
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
        public static void UpdateHash(List<BundleGroup> groups, AssetBundleManifest _main)
        {
            var bundles = _main.GetAllAssetBundles().ToList();
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                group.hash = bundles.First(x => x.StartsWith(group.hash));
            }
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var dps = _main.GetAllDependencies(group.hash);
                group.dependence = dps.ToList();
            }
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                group.usage = groups.FindAll(x => x.dependence.Contains(group.hash)).ConvertAll(x => x.hash);

            }

        }
        protected override void OnExecute(AssetTaskContext context)
        {
            var source = context.allBundleGroups;
            if (source.Count != 0)
            {
                AssetBundleManifest _main = BuildPipeline.BuildAssetBundles(context.historyPath,
                     source.ConvertAll(x => x.ToAssetBundleBuild()).ToArray(), context.BuildOption | BuildAssetBundleOptions.DryRunBuild, context.buildTarget);
                UpdateHash(source, _main);
            }
            context.manifest = BuildManifest(context.allBundleGroups, context.tree);
            InvokeComplete();
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class CollectHashBundleGroupTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {

            List<EditorAssetData> assets = context.tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            var hashMap = assets.ToDictionary(x => x.path, y => y.dps.ConvertAll(x => context.tree.GetAssetData(x).hash));
            var builds = new List<BundleGroup>();

            context.assetBuild.Create(context.tags, context.needBuildAssets.FindAll(x => x.type != AssetType.SpriteAtlas), builds);

            List<string> rawAssets = new List<string>();
            for (int i = 0; i < builds.Count; i++)
            {
                var b = builds[i];
                b.GetAssets()
                    .ToList()
                    .FindAll(x => context.assetBuild.GetAssetType(x) == AssetType.Raw)
                    .ForEach(x =>
                    {
                        b.RemoveAsset(x);
                        rawAssets.Add(x);
                    });
            }
            context.rawAssets = rawAssets;

            builds.RemoveAll(x => x.assetCount == 0);
            builds.Sort((a, b) =>
            {
                return a.length > b.length ? -1 : 1;
            });

            for (int i = 0; i < builds.Count; i++)
            {
                BundleGroup build = builds[i];
                build.CalcHash(hashMap);
            }


            context.allBundleGroups = builds;
            foreach (BundleGroup group in builds)
            {
                GetDependenceBundleGroup(context, group);
                GetUsageBundleGroup(context, group);
            }
            InvokeComplete();
        }
        private BundleGroup GetBundleGroupByAssetPath(List<BundleGroup> previewBundles, string assetPath)
        {
            return previewBundles.Find(x => x.ContainsAsset(assetPath));
        }
        private void GetDependenceBundleGroup(AssetTaskContext context, BundleGroup group)
        {
            foreach (var assetPath in group.GetAssets())
            {
                EditorAssetData data = context.tree.GetAssetData(assetPath);
                if (data != null)
                {
                    var dps = data.dps;
                    foreach (var dp in dps)
                    {
                        BundleGroup _group = GetBundleGroupByAssetPath(context.allBundleGroups, dp);
                        if (!group.dependence.Contains(_group.hash))
                            group.dependence.Add(_group.hash);
                        GetDependenceBundleGroup(context, _group);
                    }
                }

            }
        }

        private void GetUsageBundleGroup(AssetTaskContext context, BundleGroup group)
        {
            foreach (var item in group.GetAssets())
            {
                var asset = context.tree.GetAssetData(item);
                foreach (var assetPath in asset.usage)
                {
                    var g = GetBundleGroupByAssetPath(context.allBundleGroups, assetPath);
                    if (g == null) continue;
                    string hash = g.hash;
                    if (group.usage.Contains(hash)) continue;
                    group.usage.Add(hash);
                }
            }
        }

    }
}

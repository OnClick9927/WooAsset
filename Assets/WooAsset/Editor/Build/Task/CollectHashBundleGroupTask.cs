using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class CollectHashBundleGroupTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {

            List<EditorAssetData> assets = context.tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            var hashMap = assets.ToDictionary(x => x.path, y => y.dependence.ConvertAll(x => context.tree.GetAssetData(x).hash));
            var builds = new List<BundleGroup>();

            var raws = context.needBuildAssets.FindAll(x => x.type == AssetType.Raw);
            context.needBuildAssets.RemoveAll(x => raws.Contains(x));
            context.assetBuild.Create(context.needBuildAssets, builds);

            //List<string> rawAssets = new List<string>();
            //for (int i = 0; i < builds.Count; i++)
            //{
            //    var b = builds[i];
            //    b.GetAssets()
            //        .ToList()
            //        .FindAll(x => context.assetBuild.GetAssetType(x) == AssetType.Raw)
            //        .ForEach(x =>
            //        {
            //            b.RemoveAsset(x);
            //            rawAssets.Add(x);
            //        });
            //}
            //context.rawAssets = rawAssets;

            builds.RemoveAll(x => x.assetCount == 0);
            builds.Sort((a, b) =>
            {
                return a.length > b.length ? -1 : 1;
            });
            foreach (var asset in raws)
                builds.Add(BundleGroup.CreateRaw(asset));
            for (int i = 0; i < builds.Count; i++)
            {
                BundleGroup build = builds[i];
                build.CalcHash(hashMap);
            }


            context.allBundleGroups = builds;
            foreach (BundleGroup group in builds)
                GetDependenceBundleGroup(context, group);
            foreach (BundleGroup group in builds)
                GetUsageBundleGroup(context, group);
            InvokeComplete();
        }
        private BundleGroup GetBundleGroupByAssetPath(List<BundleGroup> previewBundles, string assetPath)
        {
            return previewBundles.Find(x => x.ContainsAsset(assetPath));
        }
        private void GetDependenceBundleGroup(AssetTaskContext context, BundleGroup group)
        {
            group.dependence.Clear();
            var result = group.GetAssets()
                   .Select(x => context.tree.GetAssetData(x))
                   .SelectMany(x => x.dependence)
                   .Distinct()
                   .Select(x => GetBundleGroupByAssetPath(context.allBundleGroups, x))
                   .Distinct()
                   .Select(x => x.hash);
            group.dependence.AddRange(result);
        }

        private void GetUsageBundleGroup(AssetTaskContext context, BundleGroup group)
        {
            group.usage.Clear();
            var result = context.allBundleGroups
                .FindAll(x => x.dependence.Contains(group.hash))
                .Select(x => x.hash);
            group.usage.AddRange(result);
        }

    }
}

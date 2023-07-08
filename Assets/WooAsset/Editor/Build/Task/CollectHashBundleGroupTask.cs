using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class CollectHashBundleGroupTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {

            List<EditorAssetData> assets = context.tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            var dps = assets.ToDictionary(x => x, y => assets.FindAll(a => { return a.dps.Contains(y.path); }));
            var hashMap = assets.ToDictionary(x => x.path, y => y.dps.ConvertAll(x => context.tree.GetAssetData(x).hash));
            var builds = new List<BundleGroup>();

            context.assetBuild.Create(context.tags, context.needBuildAssets.FindAll(x => x.type != AssetType.SpriteAtlas), dps, builds);

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
            InvokeComplete();
        }
    }
}

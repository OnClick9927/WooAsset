using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class CollectHashBundleGroupTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {

            List<EditorAssetData> assets = context.assetsCollection.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            var hashMap = assets.ToDictionary(x => x.path, y => y.dependence.ConvertAll(x => context.assetsCollection.GetAssetData(x).hash));
            var builds = new List<EditorBundleData>();

            var raws = context.needBuildAssets.FindAll(x => x.type == AssetType.Raw);
            context.needBuildAssets.RemoveAll(x => raws.Contains(x));
            context.assetBuild.Create(context.needBuildAssets, builds, context.buildPkg);

            builds.RemoveAll(x => x.GetIsEmpty());
            builds.Sort((a, b) =>
            {
                return a.length > b.length ? -1 : 1;
            });
            foreach (var asset in raws)
                builds.Add(EditorBundleData.CreateRaw(asset));
            for (int i = 0; i < builds.Count; i++)
            {
                EditorBundleData build = builds[i];
                build.CalculateHash(hashMap);
            }


            foreach (EditorBundleData group in builds)
                group.FindDependence(builds, context.assetsCollection.GetAllAssets());
            foreach (EditorBundleData group in builds)
                group.FindUsage(builds);

            foreach (EditorBundleData group in builds)
                if (group.CheckLoop(builds))
                    AssetsHelper.LogError($"Bundle Contains Loop {group.hash}");

            foreach (EditorBundleData group in builds)
            {
                var en = context.assetBuild.GetBundleEncrypt(context.buildPkg, group, context.encrypt);
                int code = context.assetBuild.GetEncryptCode(en);
                group.SetEncryptCode(code);
            }

            context.allBundleBuilds = builds;
            InvokeComplete();

        }
    }
}

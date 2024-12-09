using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace WooAsset
{
    public class CollectHashBundleGroupTask : AssetTask
    {
        private List<EditorBundleData> Sort(List<EditorBundleData> builds)
        {
            builds.RemoveAll(x => x.GetIsEmpty());
            builds.Sort((a, b) =>
            {
                return a.length > b.length ? -1 : 1;
            });
            return builds;
        }
        private List<EditorBundleData> HashDP(List<EditorBundleData> builds, Dictionary<string, List<string>> hashMap, bool logLoop)
        {
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
                if (group.CheckLoop(builds) && logLoop)
                    AssetsEditorTool.LogError($"Bundle Contains Loop {group.hash}");

            return builds;
        }
        protected override void OnExecute(AssetTaskContext context)
        {

            List<EditorAssetData> assets = context.assetsCollection.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            var hashMap = assets.ToDictionary(x => x.path, y => y.dependence.ConvertAll(x => context.assetsCollection.GetAssetData(x).hash));
            var builds = new List<EditorBundleData>();

            var needbuild = new List<EditorAssetData>(context.needBuildAssets);
            var raws = needbuild.FindAll(x => x.type == AssetType.Raw);
            needbuild.RemoveAll(x => raws.Contains(x));
            context.assetBuild.Create(needbuild, builds, context.buildPkg);
            builds = Sort(builds);
            builds = HashDP(builds, hashMap, false);
            var _assets = new List<EditorAssetData>(context.needBuildAssets).Where(x => x.type != AssetType.Raw).ToList();
            builds = context.bundleOptimiser.Optimise(builds, _assets, context.buildPkg);        
            foreach (var asset in raws)
                builds.Add(EditorBundleData.CreateRaw(asset));
            
            builds = Sort(builds);
            builds = HashDP(builds, hashMap, true);


            foreach (EditorBundleData group in builds)
            {
                var en = context.assetBuild.GetBundleEncrypt(context.buildPkg, group, context.encrypt);
                int code = context.assetBuild.GetEncryptCode(en);
                group.SetEncryptCode(code);
            }

            context.allBundleBuilds[context.buildPkg.name] = new List<EditorBundleData>(builds);




            InvokeComplete();

        }
    }
}

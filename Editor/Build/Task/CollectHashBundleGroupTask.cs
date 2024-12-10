using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using static UnityEngine.Networking.UnityWebRequest;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    public class CollectHashBundleGroupTask : AssetTask
    {

        private List<EditorBundleData> RemoveEmpty(List<EditorBundleData> builds)
        {
            builds.RemoveAll(x => x.GetIsEmpty());
            return builds;
        }
        private List<EditorBundleData> Sort(List<EditorBundleData> builds)
        {
            builds.Sort((a, b) =>
            {
                return a.length > b.length ? -1 : 1;
            });
            return builds;
        }
        private List<EditorBundleData> HashLengthDULoop(List<EditorBundleData> builds, Dictionary<string, List<string>> hashMap, bool logLoop)
        {
            for (int i = 0; i < builds.Count; i++)
            {
                EditorBundleData build = builds[i];
                build.CalculateHash(hashMap);
                build.CalcLength();
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

            List<EditorAssetData> assets = new List<EditorAssetData>(context.needBuildAssets);
            var hashMap = assets.ToDictionary(x => x.path, y => y.dependence.ConvertAll(x => context.assetsCollection.GetAssetData(x).hash));

            List<EditorBundleData> result = new List<EditorBundleData>();
            EditorBundleTool.N2One(assets.FindAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant), result);
            EditorBundleTool.One2One(assets.FindAll(x => x.type == AssetType.Scene), result);
            var raws = assets.FindAll(x => x.type == AssetType.Raw);
            foreach (var asset in raws)
                result.Add(EditorBundleData.CreateRaw(asset));

            assets.RemoveAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant
            || x.type == AssetType.Scene || x.type == AssetType.Raw);




            {

                var builds = new List<EditorBundleData>();

                context.assetBuild.Create(new List<EditorAssetData>(assets), builds, context.buildPkg);
                builds = RemoveEmpty(builds);
                builds = HashLengthDULoop(builds, hashMap, false);

                for (int i = 0; i < context.optimizationCount; i++)
                {
                    builds = context.bundleOptimiser.Optimize(builds, context.buildPkg, context.assetBuild);
                    builds = RemoveEmpty(builds);
                    builds = HashLengthDULoop(builds, hashMap, false);
                }

                result.AddRange(builds);
            }

            result = RemoveEmpty(result);
            result = Sort(result);
            result = HashLengthDULoop(result, hashMap, true);



            foreach (EditorBundleData group in result)
            {
                var en = context.assetBuild.GetBundleEncrypt(context.buildPkg, group, context.encrypt);
                int code = context.assetBuild.GetEncryptCode(en);
                group.SetEncryptCode(code);
            }

            context.allBundleBuilds[context.buildPkg.name] = new List<EditorBundleData>(result);




            InvokeComplete();

        }
    }
}

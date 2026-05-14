using System.Collections.Generic;
using System.Linq;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{

    public interface ICalcEditorBundleList
    {
        List<EditorBundleData> Calc(List<EditorBundleData> builds);
    }
    public class CollectHashBundleGroupTask : AssetTask, ICalcEditorBundleList
    {
        List<EditorBundleData> ICalcEditorBundleList.Calc(List<EditorBundleData> builds)
        {
            builds.RemoveAll(x => x.GetIsEmpty());

            for (int i = 0; i < builds.Count; i++)
            {
                EditorBundleData build = builds[i];
                build.CalculateHash(hashMap);
                build.CalcLength();
            }


            foreach (EditorBundleData build in builds)
                build.FindDependence(builds, context.assetsCollection.GetAllAssets().ToDictionary(x => x.path));
            foreach (EditorBundleData group in builds)
                group.FindUsage(builds);

            foreach (EditorBundleData group in builds)
                group.CheckLoop(builds);
            return builds;
        }
        private Dictionary<string, List<string>> hashMap;

        protected override void OnExecute(AssetTaskContext context)
        {

            List<EditorAssetData> assets = new List<EditorAssetData>(context.needBuildAssets);
            if (context.bundleNameCalculateType == BundleNameCalculateType.Assets_And_Dependences)
            {
                var _hashMap = assets.ToDictionary(x => x.path, y => y.dependence.ConvertAll(x => context.assetsCollection.GetAssetData(x)));
                foreach (var path in _hashMap.Keys.ToList())
                {
                    if (_hashMap[path].Count == 0)
                    {
                        _hashMap.Remove(path);
                    }
                }
                hashMap = _hashMap.ToDictionary(x => x.Key, y => y.Value.Select(z => z.hash).ToList());
            }

            List<EditorBundleData> result = new List<EditorBundleData>();
            var shaders = assets.FindAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant || x.type == AssetType.ComputeShader);
            var raws = assets.FindAll(x => x.type == AssetType.Raw);
            var scenes = assets.FindAll(x => x.type == AssetType.Scene);
            EditorBundleTool.N2One(shaders, result);
            foreach (var asset in raws)
                result.Add(EditorBundleData.CreateRaw(asset));
            assets.RemoveAll(x => shaders.Contains(x) || raws.Contains(x) || scenes.Contains(x));
            foreach (var scene in scenes)
            {
                var bundle = EditorBundleData.Create(scene);
                bundle.IsScene = true;
                result.Add(bundle);

                //var list = scene.dependence.ConvertAll(x => assets.Find(y => y.path == x));
                //list.RemoveAll(x => x == null);
                //assets.RemoveAll(x => list.Contains(x));
                //EditorBundleTool.N2MBySize(list, result);
            }










            {

                var builds = new List<EditorBundleData>();
                {
                    var _builds = context.buildPkg.rules;
                    if (_builds != null)
                        for (int i = 0; i < _builds.Count; i++)
                        {
                            var build = _builds[i];
                            build.Build(assets, result);
                        }
                }

                context.assetBuild.Create(new List<EditorAssetData>(assets), builds, context.buildPkg);
                builds = (this as ICalcEditorBundleList).Calc(builds);

                for (int i = 0; i < context.optimizationCount; i++)
                {
                    builds = context.bundleOptimiser.Optimize(builds, this, context.buildPkg, context.assetBuild);
                    builds = (this as ICalcEditorBundleList).Calc(builds);
                }

                result.AddRange(builds);
            }

            result = (this as ICalcEditorBundleList).Calc(result);
            result.Sort((a, b) =>
            {
                return a.length > b.length ? -1 : 1;
            });

            foreach (var asset in result)
            {
                if (asset.loopDependence)
                {
                    SetErr($"Bundle Contains Loop {asset.hash}");
                    InvokeComplete();
                    return;
                    //AssetsEditorTool.LogError(this.error);
                }
            }


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

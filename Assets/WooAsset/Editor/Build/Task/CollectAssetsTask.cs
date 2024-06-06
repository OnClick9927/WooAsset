using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class CollectAssetsTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            List<string> paths = new List<string>();
            if (context.Pipeline == TaskPipelineType.BuildBundle || context.Pipeline == TaskPipelineType.EditorSimulate)
                paths.AddRange(context.buildPkg.paths);
            else
            {
                if (context.Pipeline == TaskPipelineType.PreviewAllAssets)
                    paths.AddRange(context.buildPkgs.SelectMany(x => x.paths));
                else
                    paths.AddRange(context.buildPkgs.Where(x => x.build == true).SelectMany(x => x.paths));


            }

            var tree = new AssetCollection();
            tree.ReadPaths(paths, context.assetBuild);
            List<EditorAssetData> assets = tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            context.needBuildAssets = assets;
            Dictionary<string, List<string>> tag_dic = new Dictionary<string, List<string>>();
            Dictionary<string, bool> record_dic = new Dictionary<string, bool>();
            foreach (var asset in tree.GetAllAssets())
            {
                bool record = context.Params.GetIsRecord(asset.path);
                record_dic[asset.path] = record;
            }
            foreach (var item in assets)
            {
                if (AssetsHelper.GetOrDefaultFromDictionary(tag_dic, item.path) != null) continue;
                var tags = context.Params.GetAssetTags(item.path);
                if (tags == null || tags.Count == 0) continue;
                tag_dic.Add(item.path, tags.ToList());
            }
            tree.ReadRecord(record_dic);
            tree.ReadAssetTags(tag_dic);
            context.assetsCollection = tree;
            InvokeComplete();
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class CollectAssetsTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            List<string> paths = new List<string>();
            if (context.Pipeline == TaskPipelineType.BuildBundle)
                paths.AddRange(context.buildPkg.paths);
            else
            {
                if (context.collectAllPkgs)
                    paths.AddRange(context.buildPkgs.SelectMany(x => x.paths));
                else
                    paths.AddRange(context.buildPkgs.Where(x => x.collect == true).SelectMany(x => x.paths));


            }

            var tree = new AssetsTree();
            tree.ReadPaths(paths, context.assetBuild);
            List<EditorAssetData> assets = tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            context.needBuildAssets = assets;
            Dictionary<string, List<string>> tag_dic = new Dictionary<string, List<string>>();
            foreach (var item in assets)
            {
                if (tag_dic.ContainsKey(item.path)) continue;
                var tags = context.assetBuild.GetTags(item);
                if (tags == null || tags.Count == 0) continue;
                tag_dic.Add(item.path, tags.ToList());
            }
            tree.ReadAssetTags(tag_dic);
            context.tree = tree;
            InvokeComplete();
        }
    }
}

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
                paths.Add(context.buildGroup.path);
            else
                paths.AddRange(context.buildGroups.ConvertAll(x => x.path));


            context.tree = new AssetsTree();
            context.tree.ReadPaths(paths, context.assetBuild);
            List<EditorAssetData> assets = context.tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory);
            context.needBuildAssets = assets;
            context.tags = new AssetTagCollection();
            Dictionary<string, List<string>> tag_dic = new Dictionary<string, List<string>>();
            foreach (var item in assets)
            {
                if (tag_dic.ContainsKey(item.path)) continue;
                var tags = context.assetBuild.GetTags(item);
                if (tags == null || tags.Count == 0) continue;
                tag_dic.Add(item.path, tags.ToList());
            }
            context.tags.ReadAssetTags(tag_dic);
            context.rawAssets = context.tree.GetRawAssets();
            context.rawAssets_copy = context.tree.GetRawAssets_Copy();

            InvokeComplete();
        }
    }
}

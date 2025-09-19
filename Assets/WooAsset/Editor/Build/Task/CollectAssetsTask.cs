using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class CollectAssetCrossTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {

            var trees = context.allAssetCollections.ToList();
            foreach (var item in context.allAssetCollections)
            {
                var pkg_name = item.Key;
                var tree = item.Value;
                var all = tree.GetAllAssets();

                foreach (var asset in all)
                {
                    asset.in_pkgs = trees.Where(x => x.Value.GetAssetData(asset.path) != null).Select(x => x.Key).ToList();
                }
            }
            InvokeComplete();
        }
    }

    public class CollectAssetsTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            List<string> paths = new List<string>();
            if (context.Pipeline != TaskPipelineType.PreviewAllAssets)
                paths.AddRange(context.buildPkg.paths);
            else
                paths.AddRange(context.buildPkgs.SelectMany(x => x.paths));
            var tree = new EditorAssetCollection();
            tree.ReadPaths(paths, context.assetBuild);

            List<EditorAssetData> assets = tree.GetAllAssets();
            context.needBuildAssets = assets;
            foreach (var asset in assets)
            {
                if (asset.type == AssetType.Directory) continue;
                asset.record = context.Params.GetIsRecord(asset.path);
                var tag_list = context.Params.GetAssetTags(asset.path);
                if (tag_list == null || tag_list.Count == 0) continue;
                asset.tags = tag_list;
            }
            context.assetsCollection = tree;
            if (context.buildPkg != null)
                context.allAssetCollections[context.buildPkg.name] = tree;
            InvokeComplete();

        }
    }
}

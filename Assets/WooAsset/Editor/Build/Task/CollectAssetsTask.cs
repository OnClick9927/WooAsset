using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{


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
                if (AssetsEditorTool.GetOrDefaultFromDictionary(tag_dic, item.path) != null) continue;
                var tag_list = context.Params.GetAssetTags(item.path);
                if (tag_list == null || tag_list.Count == 0) continue;
                tag_dic.Add(item.path, tag_list.ToList());
            }
            tree.ReadRecord(record_dic);
            tree.ReadAssetTags(tag_dic);
            context.assetsCollection = tree;
            if (context.buildPkg != null)
                context.allAssetCollections[context.buildPkg.name] = tree;
            InvokeComplete();
        }
    }
}

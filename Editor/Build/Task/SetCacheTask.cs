using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class SetCacheTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            //if (context.Pipeline != TaskPipelineType.BuildBundle)
            {
                var cache = AssetsEditorTool.cache;
                cache.tree_asset_all = context.assetsCollection;
                cache.Pipeline = context.Pipeline;

                //cache.pkgBundles = new List<AssetsEditorCache.PkgBundles> { };
                foreach (var pkgName in context.allAssetCollections.Keys)
                {
                    List<EditorBundleData> bundles;
                    var export = context.exports.First(x => x.pkgName == pkgName);
                    context.allBundleBuilds.TryGetValue(pkgName, out bundles);
                    export.bundles = bundles;
                    export.tree = context.allAssetCollections[pkgName];
           
                }
                cache.viewAllAssets = context.Pipeline == TaskPipelineType.PreviewAllAssets;
                cache.exports = context.exports;
                cache.manifest = context.mergedManifest;
                cache.index = -1;
                cache.Save();

            }
            InvokeComplete();
        }
    }
}

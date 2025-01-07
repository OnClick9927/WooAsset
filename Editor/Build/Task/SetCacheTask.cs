using System.Collections.Generic;

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

                cache.pkgBundles = new List<AssetsEditorCache.PkgBundles> { };
                foreach (var pkgName in context.allAssetCollections.Keys)
                {
                    List<EditorBundleData> bundles;
                    context.allBundleBuilds.TryGetValue(pkgName, out bundles);
                    cache.pkgBundles.Add(new AssetsEditorCache.PkgBundles()
                    {
                        pkgName = pkgName,
                        previewBundles = bundles,
                        tree = context.allAssetCollections[pkgName]
                    });
                }
                cache.viewAllAssets = context.Pipeline == TaskPipelineType.PreviewAllAssets;
                cache.index = -1;
                cache.manifest = context.mergedManifest;
                cache.Save();

            }
            InvokeComplete();
        }
    }
}

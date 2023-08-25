using System.Collections.Generic;

namespace WooAsset
{
    public class SetCacheTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            var cache = AssetsEditorTool.cache;
            cache.tree = context.tree;
            cache.previewBundles = context.allBundleGroups != null ? context.allBundleGroups : new List<BundleGroup>();
            cache.manifest = context.manifest;
            cache.Save();
            InvokeComplete();
        }
    }
}

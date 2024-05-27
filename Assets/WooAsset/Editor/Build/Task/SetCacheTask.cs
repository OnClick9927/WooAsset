using System.Collections.Generic;

namespace WooAsset
{
    public class SetCacheTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            var cache = AssetsEditorTool.cache;
            cache.tree = context.assetsCollection;
            cache.previewBundles = context.allBundleBuilds != null ? context.allBundleBuilds : new List<EditorBundleData>();
            cache.manifest = context.manifest;
            cache.Save();
            InvokeComplete();
        }
    }
}

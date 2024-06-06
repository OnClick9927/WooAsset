using System.Collections.Generic;

namespace WooAsset
{
    public class SetCacheTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            if (context.Pipeline != TaskPipelineType.BuildBundle)
            {
                var cache = AssetsEditorTool.cache;
                cache.tree = context.assetsCollection;
                cache.previewBundles = context.allBundleBuilds != null ? context.allBundleBuilds : new List<EditorBundleData>();
                cache.manifest = context.mergedManifest;
                cache.Save();

            }
            InvokeComplete();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using static WooAsset.ManifestData;

namespace WooAsset
{
    public class FastModeManifestTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            List<AssetData> _assets = new List<AssetData>();
            var source = context.tree.GetAllAssets().Where(x => x.type != AssetType.Directory).Select(x => x.path);
            foreach (var assetPath in source)
            {
                _assets.Add(new AssetData()
                {
                    path = assetPath,
                    bundleName = string.Empty,
                    dps = context.tree.GetAssetData(assetPath).dependence,
                    tags = context.tags.GetAssetTags(assetPath).ToList(),
                    type = context.tree.GetAssetData(assetPath).type,
                });
            }
            ManifestData manifest = new ManifestData();
            manifest.Read(_assets, context.rawAssets, context.rawAssets_copy);
            manifest.Prepare();
            context.manifest = manifest;
            InvokeComplete();
        }
    }
}

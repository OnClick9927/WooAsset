using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.Tag)]

    public class TagSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.tags != null && x.tags.Contains(param.tag));
        }
    }
}

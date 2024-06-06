using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.AssetType | AssetSelectorParamType.Tag)]
    public class AssetTypeTagSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new TagSelector().Select(new AssetTypeSelector().Select(assets, param), param);
        }
    }
}

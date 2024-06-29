using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.AssetType)]
    public class AssetTypeSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.type == param.type);
        }
    }
}

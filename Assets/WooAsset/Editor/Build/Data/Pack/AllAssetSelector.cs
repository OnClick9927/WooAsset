using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.None)]

    public class AllAssetSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new List<EditorAssetData>(assets);
        }
    }
}

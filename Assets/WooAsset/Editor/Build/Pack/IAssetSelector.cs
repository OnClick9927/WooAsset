using System.Collections.Generic;

namespace WooAsset
{
    public interface IAssetSelector
    {
        List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param);
    }
}

using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.AssetType | AssetSelectorParamType.Tag | AssetSelectorParamType.Path)]
    public class AssetTypeTagDirectoryDeepSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new AssetTypeTagSelector().Select(new DirectoryDeepSelector().Select(assets, param), param);
        }
    }
}

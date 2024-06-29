using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.AssetType | AssetSelectorParamType.Path)]
    public class AssetTypeDirectoryDeepSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new DirectoryDeepSelector().Select(new AssetTypeSelector().Select(assets, param), param);

        }
    }
}

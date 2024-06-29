using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.AssetType | AssetSelectorParamType.Path)]

    public class AssetTypeDirectoryTopSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new DirectoryTopSelector().Select(new AssetTypeSelector().Select(assets, param), param);

        }
    }
}

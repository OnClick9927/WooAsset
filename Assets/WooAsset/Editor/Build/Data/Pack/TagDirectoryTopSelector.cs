using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.Tag | AssetSelectorParamType.Path)]
    public class TagDirectoryTopSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new DirectoryTopSelector().Select(new TagSelector().Select(assets, param), param);
        }
    }
}

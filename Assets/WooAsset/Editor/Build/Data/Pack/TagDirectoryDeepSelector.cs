using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.Tag | AssetSelectorParamType.Path)]
    public class TagDirectoryDeepSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new DirectoryDeepSelector().Select(new TagSelector().Select(assets, param), param);

        }
    }
}

using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.Path)]
    public class DirectoryTopSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.directory == param.path);
        }
    }
}

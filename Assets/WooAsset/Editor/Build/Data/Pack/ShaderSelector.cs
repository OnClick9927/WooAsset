using System.Collections.Generic;

namespace WooAsset
{
    [AssetSelector(AssetSelectorParamType.None)]

    public class ShaderSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
        }
    }
}

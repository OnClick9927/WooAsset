using System.Collections.Generic;

namespace WooAsset
{
    public interface ICollectBundle
    {
        void Create(List<AssetInfo> assets, Dictionary<AssetInfo, List<AssetInfo>> dpsDic, List<BundleGroup> result);
    }
}

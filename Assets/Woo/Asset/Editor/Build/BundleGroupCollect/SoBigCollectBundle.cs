using System.Collections.Generic;
using static WooAsset.AssetInfo;
using static WooAsset.AssetsBuild;

namespace WooAsset
{
    public class SoBigCollectBundle : ICollectBundle
    {
        public void Create(List<AssetInfo> assets, Dictionary<AssetInfo, List<AssetInfo>> dpsDic, List<BundleGroup> result)
        {
            DefaultCollectBundle.OneFileBundle(assets, AssetType.Scene, result);
            DefaultCollectBundle.AllInOneBundle_ALL(assets, "so_big", result);
        }
    }

}

using System.Collections.Generic;
using static WooAsset.AssetsBuild;

namespace WooAsset
{
    public class SimpleCollectBundle : ICollectBundle
    {
        public void Create(List<AssetInfo> assets, Dictionary<AssetInfo, List<AssetInfo>> dic, List<BundleGroup> result)
        {
            DefaultCollectBundle.OneFileBundle_ALL(assets, result);
        }
    }

}

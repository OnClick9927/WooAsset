using System.Collections.Generic;

namespace WooAsset
{
    public interface IBundleOptimizer
    {
        List<EditorBundleData> Optimise(List<EditorBundleData> builds, List<EditorAssetData> assets, EditorPackageData buildPkg);
    }
    public class NoneBundleOptimizer : IBundleOptimizer
    {
        public List<EditorBundleData> Optimise(List<EditorBundleData> builds, List<EditorAssetData> assets, EditorPackageData buildPkg)
        {
            return builds;
        }
    }
}

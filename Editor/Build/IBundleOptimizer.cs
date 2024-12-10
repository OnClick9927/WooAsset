using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public interface IBundleOptimizer
    {
        List<EditorBundleData> Optimize(List<EditorBundleData> builds, EditorPackageData buildPkg, IAssetsBuild build);
    }
    public class NoneBundleOptimizer : IBundleOptimizer
    {
        public List<EditorBundleData> Optimize(List<EditorBundleData> builds, EditorPackageData buildPkg, IAssetsBuild build)
        {
            return builds;
        }
    }

    public class DefaultBundleOptimizer : IBundleOptimizer
    {
        const long maxBundleLength = 2 * 1024 * 1024;
        public List<EditorBundleData> Optimize(List<EditorBundleData> builds, EditorPackageData buildPkg, IAssetsBuild build)
        {
            var only_one = builds.FindAll(x => x.usageCount == 1);
            builds.RemoveAll(x => only_one.Contains(x));

            List<EditorAssetData> no_home = new List<EditorAssetData>();
            foreach (var x in only_one)
            {

                var usage = x.GetUsage(builds).First();
                var assets = x.GetAssetsRaw();
                for (var i = 0; i < assets.Count; i++)
                {
                    var asset = assets[i];
                    if (usage.length + asset.length >= maxBundleLength)
                    {
                        no_home.Add(asset);
                    }
                    else
                    {
                        usage.AddAssetData(asset);
                    }
                }

            }

            if (no_home.Count > 0)
            {
                List<EditorBundleData> result = new List<EditorBundleData>();
                build.Create(no_home, result, buildPkg);
                builds.AddRange(result);
            }


            return builds;
        }
    }
}

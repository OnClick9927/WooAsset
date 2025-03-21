using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public interface IBundleOptimizer
    {
        List<EditorBundleData> Optimize(List<EditorBundleData> builds, ICalcEditorBundleList calc, EditorPackageData buildPkg, IAssetsBuild build);
    }
    public class NoneBundleOptimizer : IBundleOptimizer
    {
        public List<EditorBundleData> Optimize(List<EditorBundleData> builds, ICalcEditorBundleList calc, EditorPackageData buildPkg, IAssetsBuild build)
        {
            return builds;
        }
    }

    public class DefaultBundleOptimizer : IBundleOptimizer
    {
        const long maxBundleLength = 2 * 1024 * 1024;


        private List<EditorBundleData> OneUsageCombine(List<EditorBundleData> builds, EditorPackageData buildPkg, IAssetsBuild build)
        {
            var only_one = builds.FindAll(x => x.usageCount == 1 && x.dependenceCount == 0);
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



        private List<EditorBundleData> GetUsageSame(EditorBundleData build, List<EditorBundleData> builds)
        {
            if (build.usageCount == 0) return null;

            var find = builds.FindAll(x => x.usageCount == build.usageCount && x != build);
            if (find == null || find.Count == 0) return null;
            var usage = build.GetUsage(builds);
            List<EditorBundleData> sames = new List<EditorBundleData>();
            foreach (var item in find)
            {
                var _usage = item.GetUsage(builds);
                bool same = true;
                foreach (var u in usage)
                {
                    var _find = _usage.Find(x => x.hash == u.hash);

                    if (_find == null)
                    {
                        same = false;
                        break;
                    }
                }
                if (same)
                {
                    sames.Add(item);
                }
            }
            if (sames.Count > 0) sames.Add(build);
            return sames;
        }
        private List<EditorBundleData> SameUsageCombine(List<EditorBundleData> builds, EditorPackageData buildPkg, IAssetsBuild build)
        {
            List<List<EditorBundleData>> sames = new List<List<EditorBundleData>>();
            for (var i = 0; i < builds.Count; i++)
            {
                var _build = builds[i];
                bool exist = false;
                foreach (var list in sames)
                {
                    if (list.Find(x => x.hash == _build.hash) != null)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist) continue;

                var result = GetUsageSame(_build, builds);
                if (result == null || result.Count == 0) continue;
                sames.Add(result);
            }

            foreach (var same in sames)
            {
                builds.RemoveAll(x => same.Find(y => x.hash == y.hash) != null);
                EditorBundleData tmp = null;
                for (var i = 0; i < same.Count; i++)
                {
                    var g = same[i];
                    var list = g.GetAssetsRaw();
                    for (var j = 0; j < list.Count; j++)
                    {
                        var asset = list[j];
                        if (tmp == null)
                        {
                            tmp = EditorBundleData.Create(new List<EditorAssetData>());
                            builds.Add(tmp);
                        }
                        if (tmp.length + asset.length >= maxBundleLength)
                        {
                            tmp = EditorBundleData.Create(new List<EditorAssetData>());
                            builds.Add(tmp);
                        }
                        tmp.AddAssetData(asset);
                    }
                }
            }

            return builds;
        }

        public List<EditorBundleData> Optimize(List<EditorBundleData> builds, ICalcEditorBundleList calc, EditorPackageData buildPkg, IAssetsBuild build)
        {

            builds = calc.Calc(SameUsageCombine(builds, buildPkg, build));
            builds = calc.Calc(OneUsageCombine(builds, buildPkg, build));



            return builds;
        }
    }
}

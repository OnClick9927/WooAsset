using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WooAsset
{
    public class AssetsEditorCache : AssetsScriptableObject
    {
        public AssetsTree tree = new AssetsTree();
        public List<BundleGroup> previewBundles = new List<BundleGroup>();
        public AssetTagCollection tags = new AssetTagCollection();

        public BundleGroup GetBundleGroupByAssetPath(string assetPath)
        {
            return previewBundles.Find(x => x.ContainsAsset(assetPath));
        }
        public BundleGroup GetBundleGroupByBundleName(string bundleName)
        {
            return previewBundles.Find(x => x.hash == bundleName);
        }
        public List<BundleGroup> GetDependenceBundleGroup(BundleGroup group, List<BundleGroup> result)
        {
            if (result == null)
                result = new List<BundleGroup>();
            foreach (var assetPath in group.GetAssets())
            {
                EditorAssetData data = tree.GetAssetData(assetPath);
                if (data != null)
                {
                    var dps = data.dps;
                    foreach (var dp in dps)
                    {
                        BundleGroup _group = GetBundleGroupByAssetPath(dp);
                        result.Add(_group);
                        GetDependenceBundleGroup(_group, result);
                    }
                }

            }
            return result.Distinct().ToList();
        }

        public List<BundleGroup> GetUsageBundleGroup(BundleGroup group)
        {
            var result = new List<BundleGroup>();
            foreach (var item in group.GetAssets())
            {
                var asset = tree.GetAssetData(item);
                var list = tree.GetUsage(asset);
                foreach (var dp in list)
                {
                    var g = GetBundleGroupByAssetPath(dp.path);
                    if (g==null || result.Contains(g)) continue;
                    result.Add(g);
                }
            }
            return result;
        }


    }
}

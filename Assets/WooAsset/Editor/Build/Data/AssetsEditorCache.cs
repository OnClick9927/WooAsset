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
    }
}

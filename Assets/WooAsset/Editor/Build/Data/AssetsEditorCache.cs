using System.Collections.Generic;

namespace WooAsset
{
    public class AssetsEditorCache : AssetsScriptableObject
    {
        public AssetsTree tree = new AssetsTree();
        public List<BundleGroup> previewBundles = new List<BundleGroup>();

        public ManifestData manifest;

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

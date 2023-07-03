using System.Collections.Generic;

namespace WooAsset
{
    public class AssetsEditorCache : AssetsScriptableObject
    {
        public AssetsTree tree = new AssetsTree();
        public List<BundleGroup> previewBundles = new List<BundleGroup>();
        public AssetTagCollection tags = new AssetTagCollection();
    }
}

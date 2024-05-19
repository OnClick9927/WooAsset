using System.Collections.Generic;

namespace WooAsset
{
    public class AssetsEditorCache : AssetsScriptableObject
    {
        public AssetsTree tree = new AssetsTree();
        public List<EditorBundleData> previewBundles = new List<EditorBundleData>();
        public ManifestData manifest;

        public EditorBundleData GetBundleGroupByAssetPath(string assetPath)
        {
            return previewBundles.Find(x => x.ContainsAsset(assetPath));
        }
        public EditorBundleData GetBundleGroupByBundleName(string bundleName)
        {
            return previewBundles.Find(x => x.hash == bundleName);
        }
    }
}

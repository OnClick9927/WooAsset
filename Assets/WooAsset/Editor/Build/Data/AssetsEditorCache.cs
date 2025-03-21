using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    public class AssetsEditorCache : AssetsScriptableObject
    {

        public EditorAssetCollection tree_asset_all = new EditorAssetCollection();



        [System.Serializable]
        public class PkgBundles
        {
            public string pkgName;
            public List<EditorBundleData> previewBundles = new List<EditorBundleData>();
            public EditorAssetCollection tree = new EditorAssetCollection();

        }

        private int _index = 0;
        public int index
        {
            get
            {
                _index = Mathf.Clamp(_index, 0, pkgBundles.Count);
                return _index;

            }
            set
            {
                _index = Mathf.Clamp(value, 0, pkgBundles.Count);
            }
        }


        public bool viewAllAssets;


        public List<PkgBundles> pkgBundles = new List<PkgBundles>();

        private List<EditorBundleData> previewBundles_noerr = new List<EditorBundleData>();
        private EditorAssetCollection tree_bundle_noerr = new EditorAssetCollection();

        public List<EditorBundleData> previewBundles
        {
            get
            {
                if (pkgBundles.Count == 0)
                    return previewBundles_noerr;
                return pkgBundles[index].previewBundles;

            }
        }
        public EditorAssetCollection tree_bundle
        {
            get
            {
                if (pkgBundles.Count == 0)
                    return tree_bundle_noerr;
                return pkgBundles[index].tree;

            }

        }
        public EditorAssetCollection tree_asset
        {
            get
            {
                if (viewAllAssets)
                    return tree_asset_all;
                return tree_bundle;
            }
        }


        public ManifestData manifest;
        internal TaskPipelineType Pipeline;

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

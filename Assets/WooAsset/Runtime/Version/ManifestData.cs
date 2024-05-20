

using System;
using System.Collections.Generic;
using System.Linq;


namespace WooAsset
{
    [Serializable]
    public class ManifestData
    {
        public class RTName
        {
            public string name;
            public List<string> assets;

            public void AddAsset(string path)
            {
                if (assets == null) assets = new List<string>();
                if (!assets.Contains(path))
                    assets.Add(path);
            }
        }
        public class RTTag
        {
            public string tag;
            public List<string> assets;
            public void AddAsset(string path)
            {
                if (assets == null) assets = new List<string>();
                if (!assets.Contains(path))
                    assets.Add(path);
            }
        }
        [Serializable]
        public class AssetData
        {
            public string path;
            public string bundleName;
            public List<string> tags;
            public List<string> dps;
            public AssetType type;
        }
        [System.Serializable]
        public class BundleData
        {
            public string bundleName;
            public List<string> assets;
            public List<string> dependence;
            public bool raw;
            public int enCode;
        }
        public void Read(List<AssetData> assets, List<BundleData> bundles)
        {
#if UNITY_EDITOR
            this.assets = assets;
            this.bunles = bundles;
#endif
        }

        public List<AssetData> assets = new List<AssetData>();
        public List<BundleData> bunles = new List<BundleData>();

        public void Prepare()
        {
            _bundles = bunles.ToDictionary(x => x.bundleName);
            _assets = new Dictionary<string, AssetData>();
            _tags = new Dictionary<string, RTTag>();
            _names = new Dictionary<string, RTName>();
            for (int i = 0; i < assets.Count; i++)
            {
                AssetData asset = assets[i];
                string path = asset.path;
                string assetName = AssetsHelper.GetFileName(path);

                IReadOnlyList<string> tags = asset.tags;
                string bundleName = asset.bundleName;
                _assets.Add(path, asset);
                AssetsHelper.GetFromDictionary(_names, assetName).AddAsset(path);
                if (tags == null || tags.Count == 0) continue;
                foreach (var tag in tags)
                {
                    if (string.IsNullOrEmpty(tag)) continue;

                    RTTag _tag = AssetsHelper.GetFromDictionary(_tags, tag);
                    _tag.AddAsset(path);
                }
            }
            allPaths = _assets.Keys.ToList();
            allTags = _tags.Keys.ToList();
            allBundle = _bundles.Keys.ToList();
            allName = _names.Keys.ToList();
        }
        private Dictionary<string, AssetData> _assets;
        private Dictionary<string, RTName> _names;
        private Dictionary<string, RTTag> _tags;
        private Dictionary<string, BundleData> _bundles;

        [NonSerialized] public List<string> allPaths;
        [NonSerialized] public List<string> allTags;
        [NonSerialized] public List<string> allBundle;
        [NonSerialized] public List<string> allName;

        public AssetData GetAssetData(string assetpath) => AssetsHelper.GetOrDefaultFromDictionary(_assets, assetpath);
        public List<string> GetTagAssetPaths(string tag) => AssetsHelper.GetOrDefaultFromDictionary(_tags, tag)?.assets;
        public IReadOnlyList<string> GetAssets(string bundleName) => AssetsHelper.GetOrDefaultFromDictionary(_bundles, bundleName)?.assets;
        public BundleData GetBundleData(string bundleName) => AssetsHelper.GetOrDefaultFromDictionary(_bundles, bundleName);


        public IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result)
        {
            result.Clear();
            for (int i = 0; i < allName.Count; i++)
            {
                if (!name.Contains(allName[i])) continue;
                _names.TryGetValue(allName[i], out RTName _name);
                var assets = _name?.assets;
                if (assets != null)
                    result.AddRange(assets);
            }
            return result;
        }

        public static ManifestData Merge(List<ManifestData> manifests, List<string> prefer_bundles)
        {
            ManifestData manifest = new ManifestData();
            Dictionary<string, AssetData> asset_dic = new Dictionary<string, AssetData>();
            Dictionary<string, BundleData> bundle_dic = new Dictionary<string, BundleData>();

            for (int i = 0; i < manifests.Count; i++)
            {
                for (int j = 0; j < manifests[i].assets.Count; j++)
                {
                    var asset = manifests[i].assets[j];
                    string assetPath = asset.path;
                    var origin = AssetsHelper.GetOrDefaultFromDictionary(asset_dic, assetPath);
                    if (origin != null && !prefer_bundles.Contains(origin.bundleName) && prefer_bundles.Contains(asset.bundleName))
                        asset_dic[assetPath] = asset;
                    else
                        asset_dic.Add(assetPath, asset);
                }

                for (int j = 0; j < manifests[i].bunles.Count; j++)
                {
                    var bundle = manifests[i].bunles[j];
                    string assetPath = bundle.bundleName;
                    var origin = AssetsHelper.GetOrDefaultFromDictionary(bundle_dic, assetPath);
                    if (origin != null && !prefer_bundles.Contains(origin.bundleName) && prefer_bundles.Contains(bundle.bundleName))
                        bundle_dic[assetPath] = bundle;
                    else
                        bundle_dic.Add(assetPath, bundle);
                }

            }
            manifest.bunles = bundle_dic.Values.ToList();
            manifest.assets = asset_dic.Values.ToList();
            return manifest;
        }



    }
}

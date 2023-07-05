﻿

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WooAsset
{
    [Serializable]
    public class ManifestData
    {
        public class RTName
        {
            public string name;
            public List<string> assets;
        }
        public class RTTag
        {
            public string tag;
            public List<string> bundles;
            public List<string> assets;
        }
        public class RTBundle
        {
            public string bundle;
            public List<string> tags;
            public List<string> assets;
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
        public void Read(List<AssetData> assets, List<string> raw)
        {
            if (Application.isEditor)
            {
                this.assets = assets;
                this.rawAssets = raw;
            }
        }

        [SerializeField] public List<AssetData> assets = new List<AssetData>();
        public List<string> rawAssets = new List<string>();

        public void Prepare()
        {
            _bundles = new Dictionary<string, RTBundle>();
            _assets = new Dictionary<string, AssetData>();
            _tags = new Dictionary<string, RTTag>();
            _names = new Dictionary<string, RTName>();
            for (int i = 0; i < assets.Count; i++)
            {
                AssetData asset = assets[i];
                string path = asset.path;
                string assetName = Path.GetFileName(path);

                IReadOnlyList<string> tags = asset.tags;
                string bundleName = asset.bundleName;
                _assets.Add(path, asset);
                if (!_names.ContainsKey(assetName))
                {
                    _names.Add(assetName, new RTName() { name = assetName, assets = new List<string>() });
                }
                RTName _name = _names[assetName];
                if (!_name.assets.Contains(path))
                    _name.assets.Add(path);



                if (!_bundles.ContainsKey(bundleName))
                {
                    _bundles.Add(bundleName, new RTBundle()
                    {
                        bundle = bundleName,
                        assets = new List<string>(),
                        tags = new List<string>()
                    });
                }
                RTBundle _bundle = _bundles[bundleName];
                if (!_bundle.assets.Contains(path))
                    _bundle.assets.Add(path);
                if (tags == null || tags.Count == 0) continue;
                foreach (var tag in tags)
                {
                    if (string.IsNullOrEmpty(tag)) continue;
                    if (!_tags.ContainsKey(tag))
                    {
                        _tags.Add(tag, new RTTag()
                        {
                            tag = tag,
                            assets = new List<string>(),
                            bundles = new List<string>()
                        });
                    }
                    RTTag _tag = _tags[tag];
                    if (!_tag.assets.Contains(path))
                        _tag.assets.Add(path);
                    if (!_tag.bundles.Contains(bundleName))
                        _tag.bundles.Add(bundleName);
                    if (!_bundle.tags.Contains(tag))
                        _bundle.tags.Add(tag);
                }
            }
            allPaths = _assets.Keys.ToList();
            allTags = _tags.Keys.ToList();
            allBundle = _bundles.Keys.ToList();
            allName = _names.Keys.ToList();
        }
        public Dictionary<string, AssetData> _assets;
        public Dictionary<string, RTName> _names;
        public Dictionary<string, RTTag> _tags;
        public Dictionary<string, RTBundle> _bundles;

        [NonSerialized] public List<string> allPaths;
        [NonSerialized] public List<string> allTags;
        [NonSerialized] public List<string> allBundle;
        [NonSerialized] public List<string> allName;

        public AssetType GetAssetType(string assetPath)
        {
            if (_assets.ContainsKey(assetPath))
                return _assets[assetPath].type;
            if (rawAssets.Contains(assetPath))
                return AssetType.Raw;
            return AssetType.None;
        }

        public IReadOnlyList<string> GetAssetTags(string assetPath)
        {
            if (_assets.ContainsKey(assetPath))
                return _assets[assetPath].tags;
            return null;
        }

        public List<string> GetTagAssetPaths(string tag)
        {
            if (_tags.ContainsKey(tag))
                return _tags[tag].assets;
            return null;
        }
        public List<string> GetAssetDependencies(string assetPath)
        {
            if (_assets.ContainsKey(assetPath))
                return _assets[assetPath].dps;
            return null;
        }
        public string GetBundle(string assetPath)
        {
            if (_assets.ContainsKey(assetPath))
                return _assets[assetPath].bundleName;
            return null;
        }

        public IReadOnlyList<string> GetBundles()
        {
            return allBundle;
        }
        public IReadOnlyList<string> GetAssets()
        {
            return allPaths;
        }

        public IReadOnlyList<string> GetAllTags()
        {
            return allTags;
        }


        public IReadOnlyList<string> GetAssets(string bundleName)
        {
            RTBundle bundle = null;
            _bundles.TryGetValue(bundleName, out bundle);
            return bundle?.assets;
        }


        public IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result)
        {
            var fit = allName.FindAll(x => x.Contains(name));
            result.Clear();
            for (int i = 0; i < fit.Count; i++)
            {
                RTName bundle = null;
                _names.TryGetValue(name, out bundle);
                var assets = bundle?.assets;
                if (assets != null)
                {
                    result.AddRange(assets);
                }
            }
            return result;
        }

        public static ManifestData Merge(List<ManifestData> manifests, List<string> prefer)
        {
            ManifestData manifest = new ManifestData();
            Dictionary<string, AssetData> dic = new Dictionary<string, AssetData>();
            for (int i = 0; i < manifests.Count; i++)
            {
                manifest.rawAssets.AddRange(manifests[i].rawAssets);
                for (int j = 0; j < manifests[i].assets.Count; j++)
                {
                    var asset = manifests[i].assets[j];
                    if (dic.ContainsKey(asset.path))
                    {
                        if (!prefer.Contains(dic[asset.path].bundleName) && prefer.Contains(asset.bundleName))
                            dic[asset.path] = asset;
                    }
                    else
                        dic.Add(asset.path, asset);
                }

            }
            manifest.rawAssets.Distinct();
            manifest.assets = dic.Values.ToList();
            return manifest;
        }
    }
}
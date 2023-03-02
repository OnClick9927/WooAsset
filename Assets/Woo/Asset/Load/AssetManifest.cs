

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WooAsset
{
    public class AssetManifest : ScriptableObject
    {
        public const string Path = "Assets/" + Name + ".asset";
        public const string Name = "manifest";
        [Serializable]
        public class AssetData
        {
            public string path;
            public string bundleName;
            public string tag;
            public List<string> dps;
        }

        [SerializeField] private List<AssetData> assets = new List<AssetData>();

        public void Read(Dictionary<string, string> allAssets, Dictionary<string, List<string>> assetDependence, Dictionary<string, string> tags)
        {
            if (Application.isEditor)
            {
                assets.Clear();
                foreach (var item in allAssets)
                {
                    AssetData data = new AssetData();
                    data.path = item.Key;
                    data.bundleName = item.Value;
                    if (assetDependence.ContainsKey(item.Key))
                    {
                        data.dps = assetDependence[item.Key];
                    }
                    if (tags.ContainsKey(item.Key))
                    {
                        data.tag = tags[item.Key];
                    }
                    assets.Add(data);
                }
            }
        }

        private Dictionary<string, List<string>> assetDependence;
        private Dictionary<string, string> allAssets;
        private List<string> allPaths;
        private Dictionary<string, List<string>> tagAssets;
        private Dictionary<string, string> assetTags;
        private List<string> allTags;
        public void Prepare()
        {
            tagAssets = new Dictionary<string, List<string>>();
            assetDependence = new Dictionary<string, List<string>>();
            allAssets = new Dictionary<string, string>();
            allPaths = new List<string>();
            assetTags = new Dictionary<string, string>();
            for (int i = 0; i < assets.Count; i++)
            {
                AssetData asset = assets[i];
                string path = asset.path;
                string tag = asset.tag;

                assetDependence.Add(path, asset.dps);
                allAssets.Add(path, asset.bundleName);
                allPaths.Add(path);
                if (!tagAssets.ContainsKey(tag))
                    tagAssets.Add(tag, new List<string>());
                tagAssets[tag].Add(path);
                assetTags.Add(path, tag);
            }
            allTags = tagAssets.Keys.ToList();
        }
       
        public string GetAssetTag(string assetPath)
        {
            return assetTags[assetPath];
        }
        public List<string> GetTagAssetPaths(string tag)
        {
            if (tagAssets.ContainsKey(tag))
                return tagAssets[tag];
            return null;
        }

        public List<string> GetAssetDependencies(string assetPath)
        {
            if (assetDependence.ContainsKey(assetPath))
                return assetDependence[assetPath];
            return null;
        }
        public string GetBundle(string assetPath)
        {
            if (allAssets.ContainsKey(assetPath))
                return allAssets[assetPath];
            return null;
        }

        public IReadOnlyList<string> GetAssets()
        {
            return allPaths;
        }

        public IReadOnlyList<string> GetAllTags()
        {
            return allTags;
        }

 
    }
}

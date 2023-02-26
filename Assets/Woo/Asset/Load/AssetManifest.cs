

using System;
using System.Collections.Generic;
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

        public void Read(Dictionary<string, string> allAssets, Dictionary<string, List<string>> assetdps,Dictionary<string,string> tags)
        {
            if (Application.isEditor)
            {
                assets.Clear();
                foreach (var item in allAssets)
                {
                    AssetData data = new AssetData();
                    data.path = item.Key;
                    data.bundleName = item.Value;
                    if (assetdps.ContainsKey(item.Key))
                    {
                        data.dps = assetdps[item.Key];
                    }
                    if (tags.ContainsKey(item.Key))
                    {
                        data.tag = tags[item.Key];
                    }
                    assets.Add(data);
                }
            }
        }

        private Dictionary<string, List<string>> assetdps;
        private Dictionary<string, string> allAssets;
        private List<string> allPaths;
        private Dictionary<string, List<string>> tagAssets;
        private Dictionary<string, string> assetTags;

        private void Check()
        {
            if (assetdps == null)
            {
                tagAssets = new Dictionary<string, List<string>>();
                assetdps = new Dictionary<string, List<string>>();
                allAssets = new Dictionary<string, string>();
                allPaths = new List<string>();
                assetTags = new Dictionary<string, string>();
                for (int i = 0; i < assets.Count; i++)
                {
                    AssetData asset = assets[i];
                    string path = asset.path;
                    string tag = asset.tag;

                    assetdps.Add(path, asset.dps);
                    allAssets.Add(path, asset.bundleName);
                    allPaths.Add(path);
                    if (!tagAssets.ContainsKey(tag))
                        tagAssets.Add(tag, new List<string>());
                    tagAssets[tag].Add(path);
                    assetTags.Add(path, tag);
                }
            }
        }
        public string GetAssetTag(string assetPath)
        {
            Check();
            return assetTags[assetPath];
        }
        public List<string> GetTagAssetPaths(string tag)
        {
            try
            {
                Check();
                if (tagAssets.ContainsKey(tag))
                    return tagAssets[tag];
            }
            catch (Exception e)
            {
                AssetsInternal.LogError(e.Message);
            }
            return null;
        }
        public List<string> GetAssetDependences(string assetPath)
        {
            try
            {
                Check();
                if (assetdps.ContainsKey(assetPath))
                    return assetdps[assetPath];
            }
            catch (Exception e)
            {
                AssetsInternal.LogError(e.Message);
            }
            return null;
        }
        public string GetBundle(string assetPath)
        {
            try
            {
                Check();
                if (allAssets.ContainsKey(assetPath))
                    return allAssets[assetPath];
            }
            catch (Exception e)
            {
                AssetsInternal.LogError(e.Message);
            }
            return null;
        }

        public IReadOnlyList<string> GetAssets()
        {
            Check();
            return allPaths;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WooAsset
{
    [System.Serializable]
    public class AssetTagCollection
    {
        [System.Serializable]
        private class AssetTag
        {
            public string path;
            public List<string> tags;
        }
        [SerializeField] private List<AssetTag> assets = new List<AssetTag>();


        public IReadOnlyList<string> GetTagAssetPaths(string tag)
        {
            return assets.FindAll(x => x.tags.Contains(tag)).ConvertAll(x => x.path);
        }
        public void ReadAssetTags(Dictionary<string, List<string>> tagMap)
        {
            assets.Clear();
            foreach (var item in tagMap)
            {
                assets.Add(new AssetTag
                {
                    path = item.Key,
                    tags = item.Value
                });
            }
        }
        public IReadOnlyList<string> GetAssetTags(string assetPath)
        {
            IReadOnlyList<string> result = assets.Find(x => x.path == assetPath)?.tags;
            if (result == null)
                result = new List<string>();
            return result;
        }

        public IReadOnlyList<string> GetAllTags()
        {
            return assets.ConvertAll(x => x.tags).SelectMany(x => x).Distinct().ToList();
        }
    }
}

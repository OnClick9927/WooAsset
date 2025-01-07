

using System;
using System.Collections.Generic;


namespace WooAsset
{
    [Serializable]
    public class AssetData
    {
        public string path;
        public AssetType type;
        public string guid;


        public List<string> tags;
        public string bundleName;

        internal void AddTag(string tag)
        {
            if (tags == null) tags = new List<string>();
            if (!tags.Contains(tag)) tags.Add(tag);
        }
    }
}

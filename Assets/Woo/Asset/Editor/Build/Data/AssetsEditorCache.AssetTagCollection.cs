using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsEditorCache
    {
        [System.Serializable]
        public class AssetTagCollection
        {
            [System.Serializable]
            private class AssetTag
            {
                public string path;
                public string tag;
            }
            [SerializeField]private List<AssetTag> assets = new List<AssetTag>();
            public Dictionary<string, string> GetTagDic()
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (var item in assets)
                {
                    dic.Add(item.path, item.tag);
                }
                return dic;
            }
            public List<string> GetTagAssetPaths(string tag)
            {
                return assets.FindAll(x => x.tag == tag).ConvertAll(x => x.path);
            }
            public void AddAsset(string tag, string assetPath)
            {
                var find = assets.Find(x => x.path == assetPath);
                if (find != null)
                {
                    find.tag = tag;
                }
                else
                {
                    assets.Add(new AssetTag()
                    {
                        path = assetPath,
                        tag = tag
                    });
                }
            }
            public void RemoveAsset(string assetPath)
            {
                assets.RemoveAll(x => x.path == assetPath);
            }
            public void ComparePaths(List<string> paths)
            {
                List<string> remove = new List<string>();
                foreach (var item in assets)
                {
                    if (!paths.Contains(item.path))
                    {
                        remove.Add(item.path);
                    }
                }
                foreach (var item in remove)
                {
                    RemoveAsset(item);
                }
            }

            public void CompareTags(List<string> tag_new)
            {
                assets.RemoveAll(x => !tag_new.Contains(x.tag));
            }


            public string GetTag(string assetPath)
            {
                var find = assets.Find(x => x.path == assetPath);
                if (find == null)
                {
                    return null;
                }
                return find.tag;
            }


        }


    }
}

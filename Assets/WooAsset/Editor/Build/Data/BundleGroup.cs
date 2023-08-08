using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace WooAsset
{

    [Serializable]
    public class BundleGroup
    {
        public string hash;
        public long length = 0;
        [UnityEngine.SerializeField] private List<EditorAssetData> assets = new List<EditorAssetData>();
        public int assetCount { get { return assets.Count; } }

        public void CalcHash(Dictionary<string, List<string>> hashMap)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in assets)
            {
                sb = sb.Append(item.hash).Append(AssetsHelper.GetStringHash(item.path));
                if (hashMap.ContainsKey(item.path))
                {
                    List<string> hashList = hashMap[item.path];
                    for (int i = 0; i < hashList.Count; i++)
                    {
                        sb.Append(hashList[i]);
                    }
                }
            }

            this.hash = AssetsHelper.GetStringHash(sb.ToString());
        }

        public void RemoveAsset(string path)
        {
            if (!ContainsAsset(path)) return;
            EditorAssetData _find = assets.Find(x => x.path == path);
            assets.Remove(_find);
            length -= _find.length;

        }
        public bool ContainsAsset(string assetPath) => assets.Find(x => x.path == assetPath) != null;
        public IReadOnlyList<string> GetAssets() => assets.ConvertAll(x => x.path).ToList();

        public AssetBundleBuild ToAssetBundleBuild()
        {
            return new AssetBundleBuild()
            {
                assetBundleName = hash,
                assetNames = GetAssets().ToArray()
            };
        }

        public List<string> usage = new List<string>();
        public List<string> dependence = new List<string>();
        private void CalcLength()
        {
            length = 0;
            foreach (var item in assets)
            {
                length += item.length;
            }
        }
        public static BundleGroup Create(List<EditorAssetData> assets)
        {
            BundleGroup group = new BundleGroup();
            group.assets = new List<EditorAssetData>(assets);
            group.CalcLength();
            return group;
        }
        public static BundleGroup Create(EditorAssetData asset)
        {
            BundleGroup group = new BundleGroup();
            group.assets = new List<EditorAssetData>
            {
                asset
            };
            group.CalcLength();
            return group;
        }
    }
}

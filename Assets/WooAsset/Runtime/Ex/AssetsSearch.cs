

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WooAsset
{
    public class AssetsSearch
    {

        private static List<string> _IntersectTag(string[] tags, List<string> searchList)
        {
            searchList.Clear();
            IReadOnlyList<string> assets = AssetsInternal.GetAllAssetPaths();
            for (int i = 0; i < assets.Count; i++)
            {
                var assetPath = assets[i];
                var assetTags = AssetsInternal.GetAssetTags(assetPath);
                bool add = true;
                if (tags != null)
                {
                    for (int j = 0; j < tags.Length; j++)
                        if (!assetTags.Contains(tags[j]))
                            add = false;
                }
                if (!add) continue;
                searchList.Add(assetPath);
            }
            return searchList;
        }
        public static IReadOnlyList<string> IntersectNameAndTag(string assetName, params string[] tags)
        {
            var searchList = _IntersectTag(tags, new List<string>());
            searchList.RemoveAll(x => !Path.GetFileName(x).Contains(assetName));
            return searchList;
        }
        public static IReadOnlyList<string> IntersectTag(params string[] tags)
        {
            return _IntersectTag(tags, new List<string>());
        }

        public static IReadOnlyList<string> IntersectTypeAndNameAndTag(AssetType type, string assetName, params string[] tags)
        {
            var searchList = _IntersectTag(tags, new List<string>());
            searchList.RemoveAll(x => !Path.GetFileName(x).Contains(assetName));
            searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
            return searchList;
        }
        public static IReadOnlyList<string> IntersectTypeAndTag(AssetType type, params string[] tags)
        {
            var searchList = _IntersectTag(tags, new List<string>());
            searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
            return searchList;
        }



        private static List<string> _UnionTag(string[] tags, List<string> searchList)
        {
            searchList.Clear();
            if (tags == null) return searchList;
            for (int i = 0; i < tags.Length; i++)
            {
                var assets = AssetsInternal.GetTagAssetPaths(tags[i]);
                searchList.AddRange(assets);
            }
            return searchList.Distinct().ToList();
        }
        public static IReadOnlyList<string> Union(params string[] nameOrTags)
        {
            List<string> searchList = new List<string>();
            List<string> searchList2 = new List<string>();

            searchList.Clear();
            if (nameOrTags == null) return searchList;
            for (int i = 0; i < nameOrTags.Length; i++)
            {
                var assets = AssetsInternal.GetTagAssetPaths(nameOrTags[i]);
                var assets_2 = AssetsInternal.GetAssetsByAssetName(nameOrTags[i], searchList2);
                searchList.AddRange(assets_2);
                searchList.AddRange(assets);
            }
            return searchList.Distinct().ToList();
        }
        public static IReadOnlyList<string> UnionTag(params string[] tags)
        {
            return _UnionTag(tags, new List<string>());
        }
        public static IReadOnlyList<string> UnionName(params string[] names)
        {
            List<string> searchList = new List<string>();
            List<string> searchList2 = new List<string>();

            searchList.Clear();
            if (names == null) return searchList;
            for (int i = 0; i < names.Length; i++)
            {
                var assets_2 = AssetsInternal.GetAssetsByAssetName(names[i], searchList2);
                searchList.AddRange(assets_2);
            }
            return searchList.Distinct().ToList();
        }
        public static IReadOnlyList<string> UnionNameAndTag(string assetName, params string[] tags)
        {
            var searchList = _UnionTag(tags, new List<string>());
            searchList.RemoveAll(x => !Path.GetFileName(x).Contains(assetName));
            return searchList;
        }
        public static IReadOnlyList<string> UnionTypeAndNameAndTag(AssetType type, string assetName, params string[] tags)
        {
            var searchList = _UnionTag(tags, new List<string>());
            searchList.RemoveAll(x => !Path.GetFileName(x).Contains(assetName));
            searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
            return searchList;
        }
        public static IReadOnlyList<string> UnionTypeAndTag(AssetType type, params string[] tags)
        {
            var searchList = _UnionTag(tags, new List<string>());
            searchList.RemoveAll(x => AssetsInternal.GetAssetType(x) != type);
            return searchList;
        }


        public static IReadOnlyList<string> AssetPathByType(AssetType type)
        {
            IReadOnlyList<string> assets = AssetsInternal.GetAllAssetPaths();
            return assets.Where(x => AssetsInternal.GetAssetType(x) == type).ToList();
        }

    }
}

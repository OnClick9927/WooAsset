using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class BundleGroupTool
        {
            public static Dictionary<string, List<EditorAssetData>> GroupByDir(List<EditorAssetData> list)
            {
                Dictionary<string, List<EditorAssetData>> dic = new Dictionary<string, List<EditorAssetData>>();
                foreach (EditorAssetData asset in list)
                {
                    var dir = asset.directory;
                    if (!dic.ContainsKey(dir))
                    {
                        dic.Add(dir, new List<EditorAssetData>());
                    }
                    dic[dir].Add(asset);
                }
                return dic;
            }
            public static void One2One(List<EditorAssetData> assets, List<BundleGroup> result)
            {
                foreach (var atlas in assets)
                {
                    result.Add(BundleGroup.Create(atlas));
                }
            }
            public static void N2One(List<EditorAssetData> assets, List<BundleGroup> result)
            {
                result.Add(BundleGroup.Create(assets));
            }
            public static void N2MBySize(List<EditorAssetData> assets, List<BundleGroup> result, long size = 8 * 1024 * 1024)
            {
                var big = assets.FindAll(x => x.length >= size);
                assets.RemoveAll(x => x.length >= size);
                One2One(big, result);
                if (assets.Count == 0) return;
                assets.Sort((a, b) =>
                {
                    return a.length < b.length ? 1 : -1;
                });
                List<EditorAssetData> tmp = new List<EditorAssetData>();
                long len = 0;
                foreach (var item in assets)
                {
                    if (len + item.length >= size)
                    {
                        N2One(tmp, result);
                        assets.RemoveAll(x => tmp.Contains(x));
                        tmp.Clear();
                        len = 0;
                    }
                    len += item.length;
                    tmp.Add(item);
                }
                if (tmp.Count > 0)
                {
                    N2One(tmp, result);
                }
            }
            public static void N2MBySizeAndDir(List<EditorAssetData> assets, List<BundleGroup> result)
            {
                var path_dic = GroupByDir(assets);
                foreach (var item in path_dic)
                {
                    N2MBySize(item.Value, result);
                }
            }

        }



    }

}

﻿using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class EditorBundleTool
        {
            public static Dictionary<string, List<EditorAssetData>> GroupByDir(List<EditorAssetData> list)
            {
                Dictionary<string, List<EditorAssetData>> dic = new Dictionary<string, List<EditorAssetData>>();
                foreach (EditorAssetData asset in list)
                    AssetsEditorTool.GetFromDictionary(dic, asset.directory).Add(asset);
                return dic;
            }
            public static void One2One(List<EditorAssetData> assets, List<EditorBundleData> result)
            {
                foreach (var atlas in assets)
                {
                    result.Add(EditorBundleData.Create(atlas));
                }
            }
            public static void N2One(List<EditorAssetData> assets, List<EditorBundleData> result)
            {
                result.Add(EditorBundleData.Create(assets));
            }
            public static void N2MBySize(List<EditorAssetData> assets, List<EditorBundleData> result, long size = 8 * 1024 * 1024)
            {
                var big = assets.FindAll(x => x.length >= size);
                assets.RemoveAll(x => x.length >= size);
                One2One(big, result);
                if (assets.Count == 0) return;
                for (int i = 0; i < assets.Count - 1; i++)
                {
                    for (int j = i + 1; j < assets.Count; j++)
                    {
                        var a = assets[i];
                        var b = assets[j];
                        if (a.length > b.length)
                        {
                            assets[i] = b;
                            assets[j] = a;
                        }
                    }
                }

                List<EditorAssetData> tmp = new List<EditorAssetData>();
                long len = 0;
                for (int i = 0; i < assets.Count; i++)
                {
                    var item = assets[i];
                    if (len + item.length >= size)
                    {
                        N2One(tmp, result);
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
            public static void N2MBySizeAndDir(List<EditorAssetData> assets, List<EditorBundleData> result, long size = 8 * 1024 * 1024)
            {
                var path_dic = GroupByDir(assets);
                foreach (var item in path_dic)
                {
                    N2MBySize(item.Value, result, size);
                }
            }

        }


    }

}

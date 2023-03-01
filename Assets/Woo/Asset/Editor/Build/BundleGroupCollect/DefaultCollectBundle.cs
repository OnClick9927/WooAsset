using System.Collections.Generic;
using System.IO;
using static WooAsset.AssetInfo;

namespace WooAsset
{
    partial class AssetsBuild
    {
        public class DefaultCollectBundle : ICollectBundle
        {
            private static Dictionary<string, List<AssetInfo>> MakeDirDic(List<AssetInfo> list)
            {
                Dictionary<string, List<AssetInfo>> dic = new Dictionary<string, List<AssetInfo>>();
                foreach (AssetInfo asset in list)
                {
                    var dir = asset.parentPath;
                    if (!dic.ContainsKey(dir))
                    {
                        dic.Add(dir, new List<AssetInfo>());
                    }
                    dic[dir].Add(asset);
                }
                return dic;
            }
            public static void OneFileBundle_ALL(List<AssetInfo> assets, List<BundleGroup> result)
            {
                foreach (var atlas in assets)
                {
                    var file_name = Path.GetFileNameWithoutExtension(atlas.path);
                    BundleGroup atlasBundle = new BundleGroup(AssetsInternal.CombinePath(atlas.parentPath, file_name).ToAssetsPath());
                    atlasBundle.AddAsset(atlas.path);
                    result.Add(atlasBundle);
                }

            }
            public static void AllInOneBundle_ALL(List<AssetInfo> assets, string bundleName, List<BundleGroup> result)
            {
                BundleGroup shaderBundle = new BundleGroup(bundleName);
                for (int i = 0; i < assets.Count; i++)
                    shaderBundle.AddAsset(assets[i].path);
                result.Add(shaderBundle);
            }
            public static void SizeBundle_ALL(string baseName, List<AssetInfo> assets, Dictionary<AssetInfo, List<AssetInfo>> dependentMap, List<BundleGroup> result)
            {
                var find = assets.FindAll(x => dependentMap[x].Count >= 2);
                assets.RemoveAll(x => dependentMap[x].Count >= 2);
                OneFileBundle_ALL(find, result);
                if (find.Count == assets.Count) return;

                long size = setting.bundleSize;
                var tmp = assets.ConvertAll(x => { return new { info = x, length = new FileInfo(x.path).Length }; });
                var _find = tmp.FindAll(x => x.length >= size);
                OneFileBundle_ALL(_find.ConvertAll(x => x.info), result);
                tmp.RemoveAll(x => x.length >= size);


                tmp.Sort((a, b) =>
                {
                    return a.length < b.length ? 1 : -1;
                });
                Dictionary<int, List<string>> dic = new Dictionary<int, List<string>>();
                int index = 0;
                long len = 0;
                for (int i = 0; i < tmp.Count; i++)
                {
                    len += tmp[i].length;
                    if (len >= size)
                    {
                        len = 0;
                        index++;
                    }
                    if (!dic.ContainsKey(index)) dic[index] = new List<string>();
                    dic[index].Add(tmp[i].info.path);
                }

                foreach (var _index in dic)
                {
                    BundleGroup lastBundle = new BundleGroup($"{baseName}_{_index.Key}");
                    foreach (var path in _index.Value)
                    {
                        lastBundle.AddAsset(path);
                    }
                    result.Add(lastBundle);
                }
            }
            public static void SizeAndTopDirBundle_ALL(List<AssetInfo> assets, Dictionary<AssetInfo, List<AssetInfo>> dependentMap, List<BundleGroup> result)
            {
                var path_dic = MakeDirDic(assets);
                foreach (var item in path_dic)
                {
                    SizeBundle_ALL(item.Key, item.Value, dependentMap, result);
                }
            }


            public static void OneFileBundle(List<AssetInfo> assets, AssetType type, List<BundleGroup> result)
            {
                List<AssetInfo> spriteAtlas = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                OneFileBundle_ALL(spriteAtlas, result);
            }
            public static void OneTopDirBundle(List<AssetInfo> assets, AssetType type, Dictionary<AssetInfo, List<AssetInfo>> dic, List<BundleGroup> result)
            {
                List<AssetInfo> spriteAtlas = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                SizeAndTopDirBundle_ALL(spriteAtlas, dic, result);
            }
            public static void TypeSizeBundle(List<AssetInfo> assets, AssetType type, Dictionary<AssetInfo, List<AssetInfo>> dic, List<BundleGroup> result)
            {
                List<AssetInfo> spriteAtlas = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                SizeBundle_ALL(type.ToString(), spriteAtlas, dic, result);
            }

            public static void TypeAllTFileBundle(List<AssetInfo> assets, AssetType type, List<BundleGroup> result)
            {
                List<AssetInfo> shaders = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                AllInOneBundle_ALL(shaders, type.ToString(), result);
            }
            public static void TagSizeBundle(List<AssetInfo> assets, string tag, Dictionary<AssetInfo, List<AssetInfo>> dependentMap, List<BundleGroup> result)
            {
                List<AssetInfo> find = assets.FindAll(x => cache.GetAssetTag(x.path) == tag);
                assets.RemoveAll(x => cache.GetAssetTag(x.path) == tag);
                OneFileBundle(find, AssetType.Scene, result);
                SizeBundle_ALL($"tag_bundle_{tag}", find, dependentMap, result);
            }
            public static void TagSizeAndTopDirBundle(List<AssetInfo> assets, string tag, Dictionary<AssetInfo, List<AssetInfo>> dependentMap, List<BundleGroup> result)
            {
                List<AssetInfo> find = assets.FindAll(x => cache.GetAssetTag(x.path) == tag);
                assets.RemoveAll(x => cache.GetAssetTag(x.path) == tag);
                OneFileBundle(find, AssetType.Scene, result);
                SizeAndTopDirBundle_ALL(find, dependentMap, result);
            }

            public void Create(List<AssetInfo> assets, Dictionary<AssetInfo, List<AssetInfo>> dic, List<BundleGroup> result)
            {
                TypeAllTFileBundle(assets, AssetType.Shader, result);
                OneFileBundle(assets, AssetType.Scene, result);

                var tags = setting.tags;
                foreach (var tag in tags)
                {
                    TagSizeBundle(assets, tag, dic, result);
                }
                TypeSizeBundle(assets, AssetType.TextAsset, dic, result);
                OneTopDirBundle(assets, AssetType.Texture, dic, result);
                OneFileBundle(assets, AssetType.Font, result);
                OneFileBundle(assets, AssetType.SpriteAtlas, result);
                OneFileBundle(assets, AssetType.AudioClip, result);
                OneFileBundle(assets, AssetType.VideoClip, result);
                OneFileBundle(assets, AssetType.Prefab, result);
                OneFileBundle(assets, AssetType.Model, result);
                OneFileBundle(assets, AssetType.Animation, result);
                OneFileBundle(assets, AssetType.ScriptObject, result);
                OneTopDirBundle(assets, AssetType.Material, dic, result);
                SizeAndTopDirBundle_ALL(assets, dic, result);
            }
        }

    }

}

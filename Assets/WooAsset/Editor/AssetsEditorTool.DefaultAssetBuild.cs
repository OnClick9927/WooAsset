using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.U2D;
using UnityEngine.Video;
using UnityEngine;
using UnityEditor.Animations;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class DefaultAssetBuild : IAssetBuild
        {
            private static Dictionary<string, List<EditorAssetData>> MakeDirDic(List<EditorAssetData> list)
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
            public static void OneFileBundle_ALL(List<EditorAssetData> assets, List<BundleGroup> result)
            {
                foreach (var atlas in assets)
                {
                    result.Add(BundleGroup.Create(atlas));
                }

            }
            public static void AllInOneBundle_ALL(List<EditorAssetData> assets, List<BundleGroup> result)
            {
                result.Add(BundleGroup.Create(assets.ConvertAll(x => x as FileData)));
            }
            public static void SizeBundle_ALL(List<EditorAssetData> assets, Dictionary<EditorAssetData, List<EditorAssetData>> dependentMap, List<BundleGroup> result)
            {
                var find = assets.FindAll(x => dependentMap[x].Count >= 2);
                assets.RemoveAll(x => dependentMap[x].Count >= 2);
                OneFileBundle_ALL(find, result);
                if (find.Count == assets.Count) return;

                long size = option.bundleSize;
                var tmp = assets.ConvertAll(x => { return new { info = x, length = new FileInfo(x.path).Length }; });
                var _find = tmp.FindAll(x => x.length >= size);
                OneFileBundle_ALL(_find.ConvertAll(x => x.info), result);
                tmp.RemoveAll(x => x.length >= size);


                tmp.Sort((a, b) =>
                {
                    return a.length < b.length ? 1 : -1;
                });
                Dictionary<int, List<EditorAssetData>> dic = new Dictionary<int, List<EditorAssetData>>();
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
                    if (!dic.ContainsKey(index)) dic[index] = new List<EditorAssetData>();
                    dic[index].Add(tmp[i].info);
                }

                foreach (var _index in dic)
                {
                    result.Add(BundleGroup.Create(_index.Value.ConvertAll(x => x as FileData)));
                }
            }
            public static void SizeAndTopDirBundle_ALL(List<EditorAssetData> assets, Dictionary<EditorAssetData, List<EditorAssetData>> dependentMap, List<BundleGroup> result)
            {
                var path_dic = MakeDirDic(assets);
                foreach (var item in path_dic)
                {
                    SizeBundle_ALL(item.Value, dependentMap, result);
                }
            }


            public static void OneFileBundle(List<EditorAssetData> assets, AssetType type, List<BundleGroup> result)
            {
                List<EditorAssetData> spriteAtlas = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                OneFileBundle_ALL(spriteAtlas, result);
            }
            public static void OneTopDirBundle(List<EditorAssetData> assets, AssetType type, Dictionary<EditorAssetData, List<EditorAssetData>> dic, List<BundleGroup> result)
            {
                List<EditorAssetData> spriteAtlas = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                SizeAndTopDirBundle_ALL(spriteAtlas, dic, result);
            }
            public static void TypeSizeBundle(List<EditorAssetData> assets, AssetType type, Dictionary<EditorAssetData, List<EditorAssetData>> dic, List<BundleGroup> result)
            {
                List<EditorAssetData> spriteAtlas = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                SizeBundle_ALL(spriteAtlas, dic, result);
            }

            public static void TypeAllTFileBundle(List<EditorAssetData> assets, AssetType type, List<BundleGroup> result)
            {
                List<EditorAssetData> shaders = assets.FindAll(x => x.type == type);
                assets.RemoveAll(x => x.type == type);
                AllInOneBundle_ALL(shaders, result);
            }
            public static void TagSizeBundle(AssetTagCollection tags, List<EditorAssetData> assets, string tag, Dictionary<EditorAssetData, List<EditorAssetData>> dependentMap, List<BundleGroup> result)
            {
                List<EditorAssetData> find = assets.FindAll(x => tags.GetAssetTags(x.path).Contains(tag));
                assets.RemoveAll(x => tags.GetAssetTags(x.path).Contains(tag));
                OneFileBundle(find, AssetType.Scene, result);
                SizeBundle_ALL(find, dependentMap, result);
            }
            public static void TagSizeAndTopDirBundle(AssetTagCollection tags, List<EditorAssetData> assets, string tag, Dictionary<EditorAssetData, List<EditorAssetData>> dependentMap, List<BundleGroup> result)
            {
                List<EditorAssetData> find = assets.FindAll(x => tags.GetAssetTags(x.path).Contains(tag));
                assets.RemoveAll(x => tags.GetAssetTags(x.path).Contains(tag));
                OneFileBundle(find, AssetType.Scene, result);
                SizeAndTopDirBundle_ALL(find, dependentMap, result);
            }

            public void Create(AssetTagCollection tags, List<EditorAssetData> assets, Dictionary<EditorAssetData, List<EditorAssetData>> dic, List<BundleGroup> result)
            {
                TypeAllTFileBundle(assets, AssetType.Shader, result);
                OneFileBundle(assets, AssetType.Scene, result);
                foreach (var tag in tags.GetAllTags())
                {
                    TagSizeBundle(tags, assets, tag, dic, result);
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
                OneFileBundle(assets, AssetType.AnimationClip, result);
                OneFileBundle(assets, AssetType.AnimatorController, result);

                OneFileBundle(assets, AssetType.ScriptObject, result);
                OneTopDirBundle(assets, AssetType.Material, dic, result);
                SizeAndTopDirBundle_ALL(assets, dic, result);
            }

            public IReadOnlyList<string> GetTags(EditorAssetData info)
            {
                return new string[] { info.type.ToString(), Path.GetFileNameWithoutExtension(info.path) };
            }

            public List<AssetTask> GetPipelineFinishTasks(AssetTaskContext context)
            {
                return null;
            }

            public string GetVersion(string settingVersion, AssetTaskContext context)
            {
                return DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            }

            public AssetType GetAssetType(string path)
            {
                AssetType _type = AssetType.None;
                if (AssetsEditorTool.IsDirectory(path))
                {
                    _type = AssetType.Directory;
                }
                else
                {
                    AssetImporter importer = AssetImporter.GetAtPath(path);
                    if (path.EndsWith(".prefab")) _type = AssetType.Prefab;
                    else if (importer is ModelImporter) _type = AssetType.Model;
                    else if (AssetDatabase.LoadAssetAtPath<RawObject>(path) != null) _type = AssetType.RawObject;
                    else if (AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path) != null) _type = AssetType.Scene;
                    else if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null) _type = AssetType.ScriptObject;
                    else if (AssetDatabase.LoadAssetAtPath<Animation>(path) != null) _type = AssetType.Animation;
                    else if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null) _type = AssetType.AnimationClip;
                    else if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null) _type = AssetType.AnimatorController;
                    else if (AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path) != null) _type = AssetType.SpriteAtlas;
                    else if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) _type = AssetType.Material;
                    else if (AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null) _type = AssetType.AudioClip;
                    else if (AssetDatabase.LoadAssetAtPath<VideoClip>(path) != null) _type = AssetType.VideoClip;
                    else if (AssetDatabase.LoadAssetAtPath<Texture>(path) != null) _type = AssetType.Texture;
                    else if (AssetDatabase.LoadAssetAtPath<Font>(path) != null) _type = AssetType.Font;
                    else if (AssetDatabase.LoadAssetAtPath<Shader>(path) != null) _type = AssetType.Shader;
                    else if (AssetDatabase.LoadAssetAtPath<TextAsset>(path) != null) _type = AssetType.TextAsset;
                    else if (AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path) != null) _type = AssetType.ShaderVariant;
                    else if (AssetDatabase.LoadAssetAtPath<DefaultAsset>(path) != null) _type = AssetType.Raw;
                }
                return _type;
            }
        }



    }

}

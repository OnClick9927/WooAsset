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
            public void Create(AssetTagCollection tags, List<EditorAssetData> assets, List<BundleGroup> result)
            {
                List<EditorAssetData> Shaders = assets.FindAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                assets.RemoveAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                BundleGroupTool.N2One(Shaders, result);

                List<EditorAssetData> Scenes = assets.FindAll(x => x.type == AssetType.Scene);
                assets.RemoveAll(x => x.type == AssetType.Scene);
                BundleGroupTool.One2One(Scenes, result);

                foreach (var tag in tags.GetAllTags())
                {
                    List<EditorAssetData> find = assets.FindAll(x => tags.GetAssetTags(x.path).Contains(tag));
                    assets.RemoveAll(x => tags.GetAssetTags(x.path).Contains(tag));
                    BundleGroupTool.N2MBySize(find, result);
                }
                List<AssetType> _n2mSize = new List<AssetType>() {
                    AssetType.TextAsset
                };
                List<AssetType> _n2mSizeDir = new List<AssetType>() {
                     AssetType.Texture,
                     AssetType.Material,
                };
                List<AssetType> _one2one = new List<AssetType>() {
                    AssetType.Font,
                    AssetType.AudioClip,
                    AssetType.VideoClip,
                    AssetType.Prefab,
                    AssetType.Model,
                    AssetType.Animation,
                    AssetType.AnimationClip,
                    AssetType.AnimatorController,
                    AssetType.ScriptObject,
                };
                foreach (var item in _one2one)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    BundleGroupTool.One2One(fits, result);
                }
                foreach (var item in _n2mSize)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    BundleGroupTool.N2MBySize(fits, result);
                }
                foreach (var item in _n2mSizeDir)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    BundleGroupTool.N2MBySizeAndDir(fits, result);
                }
                BundleGroupTool.N2MBySizeAndDir(assets, result);
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
                    if (path.EndsWith(".meta")) _type = AssetType.Meta;
                    else if (path.EndsWith(".cs")) _type = AssetType.CS;
                    else if (path.EndsWith(".prefab")) _type = AssetType.Prefab;
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

            public bool IsIgnorePath(string path)
            {
                var type = GetAssetType(path);
                if (type == AssetType.Meta || type == AssetType.CS || type == AssetType.SpriteAtlas || type == AssetType.Raw)
                    return true;
                var list = AssetsInternal.ToRegularPath(path).Split('/').ToList();
                if (!list.Contains("Assets") || list.Contains("Editor") || list.Contains("Resources")) return true;
                return false;
            }
        }



    }

}

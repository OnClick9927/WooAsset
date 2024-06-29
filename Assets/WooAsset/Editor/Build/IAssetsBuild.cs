using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.U2D;
using UnityEngine.Video;
using UnityEngine;
using UnityEditor.Animations;
using static WooAsset.AssetsEditorTool;
using UnityEngine.Audio;
using System;


namespace WooAsset
{
    public abstract class IAssetsBuild
    {
        public virtual bool GetIsRecord(string path) => true;

        public virtual List<string> GetAssetTags(string path) => null;
        public virtual string GetVersion(string settingVersion, AssetTaskContext context) => settingVersion;
        protected virtual AssetType CoverAssetType(string path, AssetType type) => type;
        public AssetType GetAssetType(string path)
        {
            var list = AssetsHelper.ToRegularPath(path).Split('/').ToList();
            if (!list.Contains("Assets") || list.Contains("Editor") || list.Contains("Resources")) return AssetType.Ignore;
            AssetType _type = AssetType.None;
            if (AssetsEditorTool.IsDirectory(path))
                _type = AssetType.Directory;
            else
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var type = obj?.GetType();
                if (obj == null) _type = AssetType.Ignore;
                else if (type == typeof(MonoScript)) _type = AssetType.Ignore;
                else if (type == typeof(LightingDataAsset)) _type = AssetType.Ignore;
                else if (type == typeof(SpriteAtlas)) _type = AssetType.Ignore;
                else if (type == typeof(UnityEditor.SceneAsset)) _type = AssetType.Scene;
                else if (type == typeof(GameObject)) _type = AssetType.GameObject;
                else if (type == typeof(Animation)) _type = AssetType.Animation;
                else if (type == typeof(AnimationClip)) _type = AssetType.AnimationClip;
                else if (type == typeof(AnimatorController)) _type = AssetType.AnimatorController;
                else if (type == typeof(Font)) _type = AssetType.Font;
                else if (type == typeof(Mesh)) _type = AssetType.Mesh;
                else if (type == typeof(Material)) _type = AssetType.Material;
                else if (type == typeof(AudioClip)) _type = AssetType.AudioClip;
                else if (type == typeof(VideoClip)) _type = AssetType.VideoClip;
                else if (type == typeof(TextAsset)) _type = AssetType.TextAsset;
                else if (type == typeof(Shader)) _type = AssetType.Shader;
                else if (type == typeof(ShaderVariantCollection)) _type = AssetType.ShaderVariant;
                else if (type == typeof(ComputeShader)) _type = AssetType.ComputeShader;
                else if (type == typeof(PhysicMaterial)) _type = AssetType.PhysicMaterial;
                else if (type == typeof(AudioMixer)) _type = AssetType.AudioMixer;
                else if (type == typeof(GUISkin)) _type = AssetType.GUISkin;
                else if (type == typeof(DefaultAsset)) _type = AssetType.Raw;
                else if (typeof(ScriptableObject).IsAssignableFrom(type)) _type = AssetType.ScriptObject;
                else if (typeof(Texture).IsAssignableFrom(type))
                {
                    TextureImporter assetImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    _type = AssetType.Texture;
                    if (assetImporter.textureType == TextureImporterType.Sprite)
                        _type = AssetType.Sprite;
                }
                _type = CoverAssetType(path, _type);
            }
            return _type;
        }
        public virtual void Create(List<EditorAssetData> assets, List<EditorBundleData> result, EditorPackageData pkg)
        {
            var builds = pkg.builds;
            if (builds == null || builds.Count == 0)
            {
                List<EditorAssetData> Shaders = assets.FindAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                assets.RemoveAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                EditorBundleTool.N2One(Shaders, result);

                List<EditorAssetData> Scenes = assets.FindAll(x => x.type == AssetType.Scene);
                assets.RemoveAll(x => x.type == AssetType.Scene);
                EditorBundleTool.One2One(Scenes, result);
                var tagAssets = assets.FindAll(x => x.tags != null && x.tags.Count != 0);
                assets.RemoveAll(x => tagAssets.Contains(x));
                var tags = tagAssets.SelectMany(x => x.tags).Distinct().ToList();
                tags.Sort();
                foreach (var tag in tags)
                {
                    List<EditorAssetData> find = tagAssets.FindAll(x => x.tags.Contains(tag));
                    tagAssets.RemoveAll(x => find.Contains(x));
                    EditorBundleTool.N2MBySize(find, result);
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
                    AssetType.GameObject,
                    AssetType.Animation,
                    AssetType.AnimationClip,
                    AssetType.AnimatorController,
                    AssetType.ScriptObject,
                };
                foreach (var item in _one2one)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.One2One(fits, result);
                }
                foreach (var item in _n2mSize)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.N2MBySize(fits, result);
                }
                foreach (var item in _n2mSizeDir)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.N2MBySizeAndDir(fits, result);
                }
                EditorBundleTool.N2MBySizeAndDir(assets, result);
            }
            else
            {
                for (int i = 0; i < builds.Count; i++)
                {
                    var build = builds[i];
                    build.Build(assets, result);
                }
            }
        }

        public virtual IAssetEncrypt GetBundleEncrypt(EditorPackageData pkg, EditorBundleData data, IAssetEncrypt en) => en;
        public virtual int GetEncryptCode(IAssetEncrypt en)
        {
            if (en is NoneAssetStreamEncrypt) return NoneAssetStreamEncrypt.code;
            if (en is DefaultAssetStreamEncrypt) return DefaultAssetStreamEncrypt.code;
            return -1;
        }
        NoneAssetStreamEncrypt none = new NoneAssetStreamEncrypt();
        DefaultAssetStreamEncrypt def = new DefaultAssetStreamEncrypt();
        public virtual IAssetEncrypt GetEncryptByCode(int code)
        {
            if (code == NoneAssetStreamEncrypt.code) return none;
            if (code == DefaultAssetStreamEncrypt.code) return def;
            return null;
        }

    }

    class DefaultAssetsBuild : IAssetsBuild
    {
        public override string GetVersion(string settingVersion, AssetTaskContext context)
        {
            return DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        }

    }
}

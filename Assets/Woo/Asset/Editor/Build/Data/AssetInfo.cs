using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Video;
using UnityEngine.U2D;

namespace WooAsset
{
    [System.Serializable]
    public class AssetInfo
    {
        [System.Serializable]
        public enum AssetType
        {
            None,
            Directory,
            Texture,
            Shader,
            ShaderVariant,
            VideoClip,
            AudioClip,
            Scene,
            Material,
            Prefab,
            Font,
            Animation,
            SpriteAtlas,
            ScriptObject,
            Model,
            TextAsset,

        }
        [SerializeField] private string _path;
        [SerializeField] private string _parentPath;
        [SerializeField] private AssetType _type;
        public List<string> dps = new List<string>();
        public AssetType type { get { return _type; } }
        public string path { get { return _path; } }
        public string parentPath { get { return _parentPath; } }
        public AssetInfo(string path, string parentPath)
        {
            this._path = path;
            this._parentPath = parentPath;
            if (path.IsDirectory())
            {
                _type = AssetType.Directory;
            }
            else
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (path.EndsWith(".prefab")) _type = AssetType.Prefab;
                else if (importer is ModelImporter) _type = AssetType.Model;
                else if (AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path) != null) _type = AssetType.Scene;
                else if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null) _type = AssetType.ScriptObject;
                else if (AssetDatabase.LoadAssetAtPath<Animation>(path) != null) _type = AssetType.Animation;
                else if (AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path) != null) _type = AssetType.SpriteAtlas;
                else if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) _type = AssetType.Material;
                else if (AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null) _type = AssetType.AudioClip;
                else if (AssetDatabase.LoadAssetAtPath<VideoClip>(path) != null) _type = AssetType.VideoClip;
                else if (AssetDatabase.LoadAssetAtPath<Texture>(path) != null) _type = AssetType.Texture;
                else if (AssetDatabase.LoadAssetAtPath<Font>(path) != null) _type = AssetType.Font;
                else if (AssetDatabase.LoadAssetAtPath<Shader>(path) != null) _type = AssetType.Shader;
                else if (AssetDatabase.LoadAssetAtPath<TextAsset>(path) != null) _type = AssetType.TextAsset;
                else if (AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path) != null) _type = AssetType.ShaderVariant;
            }
        }


    }
}

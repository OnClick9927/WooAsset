using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Profiling;

namespace WooAsset
{
    [System.Serializable]
    public class EditorAssetData : FileData
    {

        public List<string> dependence = new List<string>();
        public AssetType type;
        public string directory;
        public List<string> tags;
        public List<string> usage = new List<string>();
        public bool record;

        public int usageCount { get { return usage.Count; } }
        public static EditorAssetData Create(string path, AssetType _type)
        {
            long length = 0;
            string hash;
            if (_type == AssetType.Directory)
            {
                hash = AssetsHelper.GetStringHash(path);
            }
            else
            {
                hash = AssetsHelper.GetFileHash(path);
                if (_type == AssetType.Sprite || _type == AssetType.Texture)
                    length = AssetsEditorTool.GetTextureMemorySizeLong(path);
                else
                    length = AssetsHelper.GetFileLength(path);
            }

            return new EditorAssetData()
            {
                path = path,
                name = AssetsHelper.GetFileName(path),
                length = length,
                hash = hash,
                type = _type,
                directory = AssetsEditorTool.ToAssetsPath(AssetsEditorTool.GetDirectoryName(path))
            };
        }

        public AssetData CreateAssetData(string bundleName)
        {
            return new AssetData()
            {
                path = path,
                bundleName = bundleName,
                tags = tags,
                type = type,
            };
        }
    }
}

using System.Collections.Generic;
using UnityEditor;

namespace WooAsset
{
    [System.Serializable]
    public class EditorAssetData : FileData
    {

        public List<string> dependence = new List<string>();
        public AssetType type;
        public FileType fileType => type == AssetType.Directory ? FileType.Directory : FileType.File;
        public string directory;
        public List<string> tags;
        public List<string> usage = new List<string>();
        public bool record;
        public string guid;
        public List<string> in_pkgs;

        public int usageCount { get { return usage.Count; } }
        public static EditorAssetData Create(string path, AssetType _type)
        {
            long length = 0;
            string hash;
            if (_type == AssetType.Directory)
            {
                hash = AssetsEditorTool.GetStringHash(path);
            }
            else
            {
                hash = AssetsEditorTool.GetFileHash(path);
                if (_type == AssetType.Sprite || _type == AssetType.Texture)
                    length = AssetsEditorTool.GetTextureMemorySizeLong(path);
                else
                    length = AssetsEditorTool.GetFileLength(path);
            }

            return new EditorAssetData()
            {
                path = path,
                name = AssetsEditorTool.GetFileName(path),
                length = length,
                hash = hash,
                type = _type,
                directory = AssetsEditorTool.ToAssetsPath(AssetsEditorTool.GetDirectoryName(path)),
                guid = AssetDatabase.AssetPathToGUID(path)
            };
        }

        public AssetData CreateAssetData(string bundleName)
        {
            return new AssetData()
            {
                guid = guid,
                path = path,
                bundleName = bundleName,
                tags = tags,
                type = type,
            };
        }
    }
}

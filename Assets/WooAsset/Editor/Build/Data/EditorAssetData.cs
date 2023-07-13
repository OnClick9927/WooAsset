using System.Collections.Generic;
using System.IO;

namespace WooAsset
{
    [System.Serializable]
    public class EditorAssetData : FileData
    {

        public List<string> dps = new List<string>();
        public AssetType type;
        public string directory;
        public List<string> usage = new List<string>();
        public int usageCount { get { return usage.Count; } }
        public static EditorAssetData Create(string path, AssetType _type)
        {
            string hash = string.Empty;
            long length = 0;
            if (_type == AssetType.Directory)
            {
                hash = AssetsInternal.GetStringHash(path);
            }
            else
            {
                hash = AssetsInternal.GetFileHash(path);
                length = new FileInfo(path).Length;
            }

            return new EditorAssetData()
            {
                path = path,
                name = Path.GetFileName(path),
                length = length,
                hash = hash,
                type = _type,
                directory = AssetsEditorTool.ToAssetsPath(Path.GetDirectoryName(path))
            };
        }
    }
}

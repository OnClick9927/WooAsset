using System.Collections.Generic;
using static WooAsset.ManifestData;

namespace WooAsset
{
    [System.Serializable]
    public class EditorAssetData : FileData
    {

        public List<string> dependence = new List<string>();
        public AssetType type;
        public string directory;
        public List<string> tags;
        public bool loopDependence = false;
        public List<string> usage = new List<string>();
        public int usageCount { get { return usage.Count; } }
        public static EditorAssetData Create(string path, AssetType _type)
        {
            string hash = string.Empty;
            long length = 0;
            if (_type == AssetType.Directory)
            {
                hash = AssetsHelper.GetStringHash(path);
            }
            else
            {
                hash = AssetsHelper.GetFileHash(path);
                length = AssetsHelper.GetFileLength(path);
            }

            return new EditorAssetData()
            {
                path = path,
                name = AssetsHelper.GetFileName(path),
                length = length,
                hash = hash,
                type = _type,
                directory = AssetsHelper.ToAssetsPath(AssetsHelper.GetDirectoryName(path))
            };
        }

        public AssetData CreateAssetData(string bundleName)
        {
            return new AssetData()
            {
                path = path,
                bundleName = bundleName,
                dps = dependence,
                tags = tags,
                type = type,
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    [System.Serializable]
    public class TagAssets
    {
        public string tag;
        public void Add(FileType type, string path)
        {
            if (!assets.Any(x => x.type == type && x.path == path))
                assets.Add(new FileRecordData()
                {
                    type = type,
                    path = path
                });
        }
        public bool FitAssetTag(string path) => assets.Any(x => x.Fit(path));
        public void Remove(FileType type, string path)
        {
            if (type == FileType.File)
                assets.RemoveAll(x => x.path == path && x.type == type);
            else
                assets.RemoveAll(x => x.path.StartsWith(path));
        }
        public List<FileRecordData> assets = new List<FileRecordData>();
    }
}

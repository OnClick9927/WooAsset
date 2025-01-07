using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    [System.Serializable]
    public class TagAssets
    {
        public string tag;
        [System.Serializable]
        public class TagData
        {
            public string path;
            public FileType type;
        }
        public void Add(FileType type, string path)
        {
            if (!assets.Any(x => x.type == type && x.path == path))
                assets.Add(new TagAssets.TagData()
                {
                    type = type,
                    path = path
                });
        }
        public bool FitAssetTag(string path)
        {
            return assets.Any(x => (x.type == FileType.File && x.path == path)
            || (x.type == FileType.Directory && path.StartsWith(x.path)));
        }
        public void Remove(FileType type, string path)
        {
            if (type == FileType.File)
                assets.RemoveAll(x => x.path == path && x.type == type);
            else
                assets.RemoveAll(x => x.path.StartsWith(path));
        }
        public List<TagData> assets = new List<TagData>();
    }
}

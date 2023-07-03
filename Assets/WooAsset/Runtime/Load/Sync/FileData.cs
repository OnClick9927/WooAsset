
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WooAsset
{
    [System.Serializable]
    public class FileData
    {
        public enum FileCompareType
        {
            Hash,
            Length
        }
        public string path;
        public string name;
        public long length;
        public string hash;

        public static FileData CreateByFile(string path)
        {
            return new FileData()
            {
                name = Path.GetFileName(path),
                path = path,
                length = new FileInfo(path).Length,
                hash = AssetsInternal.GetFileHash(path),
            };
        }
        public static List<FileData> Distinct(List<FileData> src)
        {
            Dictionary<string, FileData> map = new Dictionary<string, FileData>();
            for (int i = 0; i < src.Count; i++)
            {
                if (map.ContainsKey(src[i].name)) continue;
                map.Add(src[i].name, src[i]);
            }
            return map.Values.ToList();
        }

        public static bool Compare(FileData a, FileData b, FileCompareType checkType)
        {
            if (a == null || b == null)
                return false;
            if (a.name != b.name) return false;
            if (checkType == FileCompareType.Hash)
            {
                return a.hash == b.hash;
            }
            else
            {
                return a.length == b.length;
            }
        }

        public static void Compare(List<FileData> old, List<FileData> src, FileCompareType checkType, out List<FileData> change, out List<FileData> delete, out List<FileData> add)
        {
            delete = old.FindAll(x => src.Find(y => y.name == x.name) == null);
            add = src.FindAll(x => old.Find(y => y.name == x.name) == null);
            if (checkType == FileCompareType.Hash)
            {
                change = src.FindAll(x => old.Find(y => y.name == x.name && x.hash != y.hash) != null);
            }
            else
            {
                change = src.FindAll(x => old.Find(y => y.name == x.name && x.length != y.length) != null);
            }

        }
    }
}

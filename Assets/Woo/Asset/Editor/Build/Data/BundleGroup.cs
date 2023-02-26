using System;
using System.Collections.Generic;
using System.IO;

namespace WooAsset
{

    [Serializable]
    public class BundleGroup
    {
        [Serializable]

        private class Len
        {
            public string path;
            public long len;
        }
        public string name;
        public List<string> assets = new List<string>();
        [UnityEngine.SerializeField] private List<Len> lens = new List<Len>();

        public long length = 0;
        public long GetLength(string path)
        {
            return lens.Find(x => x.path == path).len;
        }
        public BundleGroup(string name)
        {
            this.name = name;
        }
        public void AddAsset(string path)
        {
            if (assets.Contains(path))
            {
                return;
            }
            assets.Add(path);
            FileInfo info = new FileInfo(path);
            length += info.Length;
            lens.Add(new Len()
            {
                path = path,
                len = info.Length
            });
        }
        public void RemoveAsset(string path)
        {
            if (!assets.Contains(path))
            {
                return;
            }
            assets.Remove(path);
            FileInfo info = new FileInfo(path);
            length -= info.Length;
            lens.RemoveAll(x => x.path == path);
        }

    }
}

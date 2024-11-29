using System.Collections.Generic;


namespace WooAsset
{
    [System.Serializable]
    public class BundleData
    {
        public string bundleName;
        public bool raw;
        public CompressType compress;
        public int enCode;
        public string hash;
        public long length;
        public string bundleHash;
        public uint bundleCrc;

        public List<string> assets;
        public List<string> dependence;

        public void AddAsset(string path)
        {
            if (assets == null)
                assets = new List<string>();
            if (assets.Contains(path)) return;
            assets.Add(path);
        }



        public static void Compare(List<BundleData> old, List<BundleData> src, out List<BundleData> change, out List<BundleData> delete, out List<BundleData> add)
        {
            delete = old.FindAll(x => src.Find(y => y.bundleName == x.bundleName) == null);
            add = src.FindAll(x => old.Find(y => y.bundleName == x.bundleName) == null);
            change = src.FindAll(x => old.Find(y => y.bundleName == x.hash && x.hash != y.hash && x.length != y.length) != null);
        }
    }
}

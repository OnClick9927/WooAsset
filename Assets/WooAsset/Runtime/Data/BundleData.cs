using System.Collections.Generic;
using static WooAsset.FileData;


namespace WooAsset
{
    [System.Serializable]
    public class BundleData : IBufferObject
    {
        [UnityEngine.SerializeField] internal List<int> bundleDependence;
        public string bundleName;
        public bool raw;
        public int enCode;
        public string hash;
        public long length;

        public void AddAsset(string path)
        {
            if (assets == null)
                assets = new List<string>();
            if (assets.Contains(path)) return;
            assets.Add(path);
        }

        void IBufferObject.ReadData(BufferReader reader)
        {
            raw = reader.ReadBool();
            enCode = reader.ReadInt32();
            length = reader.ReadInt64();

            bundleDependence = reader.ReadInt32List();
            bundleName = reader.ReadUTF8();
            hash = reader.ReadUTF8();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteBool(raw);
            writer.WriteInt32(enCode);
            writer.WriteInt64(length);

            writer.WriteInt32List(bundleDependence);
            writer.WriteUTF8(bundleName);
            writer.WriteUTF8(hash);

        }

        [System.NonSerialized] public List<string> assets;
        [System.NonSerialized] public List<string> dependence;

        public static void Compare(List<BundleData> old, List<BundleData> src, out List<BundleData> change, out List<BundleData> delete, out List<BundleData> add)
        {
            delete = old.FindAll(x => src.Find(y => y.bundleName == x.bundleName) == null);
            add = src.FindAll(x => old.Find(y => y.bundleName == x.bundleName) == null);
            change = src.FindAll(x => old.Find(y => y.bundleName == x.hash && x.hash != y.hash && x.length != y.length) != null);
        }
    }
}

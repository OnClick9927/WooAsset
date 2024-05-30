using System.Collections.Generic;


namespace WooAsset
{
    [System.Serializable]
    public class BundleData : IBufferObject
    {
        [UnityEngine.SerializeField] internal List<int> bundleDependence;
        public string bundleName;

        public bool raw;
        public int enCode;
        public void AddAsset(string path)
        {
            if (assets == null)
                assets = new List<string>();
            if (assets.Contains(path)) return;
            assets.Add(path);
        }

        void IBufferObject.ReadData(BufferReader reader)
        {
            bundleDependence = reader.ReadInt32List();
            bundleName = reader.ReadUTF8();
            raw = reader.ReadBool();
            enCode = reader.ReadInt32();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteInt32List(bundleDependence);
            writer.WriteUTF8(bundleName);

            writer.WriteBool(raw);
            writer.WriteInt32(enCode);

        }

        [System.NonSerialized] public List<string> assets;
        [System.NonSerialized] public List<string> dependence;
    }
}

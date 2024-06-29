

using System;
using System.Collections.Generic;


namespace WooAsset
{
    [Serializable]
    public class AssetData : IBufferObject
    {
        [UnityEngine.SerializeField] internal int bundleNameIndex;
        public string path;
        public AssetType type;
        public int[] path_segs;
        public int[] dot_segs;
        [System.NonSerialized] public List<string> tags;
        [System.NonSerialized] public string bundleName;

        internal void AddTag(string tag)
        {
            if (tags == null) tags = new List<string>();
            if (!tags.Contains(tag)) tags.Add(tag);
        }

        void IBufferObject.ReadData(BufferReader reader)
        {
            bundleNameIndex = reader.ReadInt32();
            //path = reader.ReadUTF8();
            type = (AssetType)reader.ReadUInt16();
            path_segs = reader.ReadInt32Array();
            dot_segs = reader.ReadInt32Array();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteInt32(bundleNameIndex);
            //writer.WriteUTF8(path);
            writer.WriteUInt16((ushort)type);
            writer.WriteInt32Array(path_segs);
            writer.WriteInt32Array(dot_segs);
        }
    }
}

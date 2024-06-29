using System.Collections.Generic;


namespace WooAsset
{
    [System.Serializable]
    internal class TagData : IBufferObject
    {
        public string tag;
        [UnityEngine.SerializeField] internal List<int> assetIndexes;
        [System.NonSerialized] public List<string> assets;

        public void AddAsset(int pathindex)
        {
            if (assetIndexes == null) assetIndexes = new List<int>();
            if (!assetIndexes.Contains(pathindex))
                assetIndexes.Add(pathindex);
        }

        void IBufferObject.ReadData(BufferReader reader)
        {
            tag = reader.ReadUTF8();
            assetIndexes = reader.ReadInt32List();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8(tag);
            writer.WriteInt32List(assetIndexes);
        }
    }
}

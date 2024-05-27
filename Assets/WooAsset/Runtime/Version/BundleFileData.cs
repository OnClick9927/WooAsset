namespace WooAsset
{
    public class BundleFileData : IBufferObject
    {
        public string name;
        public string hash;
        public long length;

        void IBufferObject.ReadData(BufferReader reader)
        {
            length = reader.ReadInt64();
            name = reader.ReadUTF8();
            hash = reader.ReadUTF8();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteInt64(length);
            writer.WriteUTF8(name);
            writer.WriteUTF8(hash);
        }
    }

}

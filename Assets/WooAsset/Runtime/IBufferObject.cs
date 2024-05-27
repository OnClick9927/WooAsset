namespace WooAsset
{
    public interface IBufferObject
    {
        void ReadData(BufferReader reader);
        void WriteData(BufferWriter writer);
    }
}

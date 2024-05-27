namespace WooAsset
{
    [System.Serializable]
    public class StreamBundleList : IBufferObject
    {
        public string[] fileNames;
        public static string fileName = $"StreamBundleList.ver{CopyStreamBundlesOperation.fileExt}";

        void IBufferObject.ReadData(BufferReader reader) => fileNames = reader.ReadUTF8Array();

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8Array(fileNames);
        }
    }
}

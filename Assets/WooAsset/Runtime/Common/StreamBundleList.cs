namespace WooAsset
{
    [System.Serializable]
    public class StreamBundleList : IBufferObject
    {
        public const string fileExt = ".bytes";

        public string[] fileNames;
        public static string fileName = $"StreamBundleList{VersionHelper.versionExt}{fileExt}";

        void IBufferObject.ReadData(BufferReader reader) => fileNames = reader.ReadUTF8Array();

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8Array(fileNames);
        }
    }
}

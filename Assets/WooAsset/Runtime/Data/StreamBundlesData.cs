namespace WooAsset
{
    [System.Serializable]
    public class StreamBundlesData : IBufferObject
    {
        public const string fileExt = ".bytes";
        public string version;
        public string[] fileNames;
        public static string fileName = $"StreamBundleList{AssetsHelper.versionExt}{fileExt}";

        void IBufferObject.ReadData(BufferReader reader)
        {
            fileNames = reader.ReadUTF8Array();
            try
            {
                version = reader.ReadUTF8();
            }
            catch (System.Exception)
            {
            }

        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8Array(fileNames);
            writer.WriteUTF8(version);


        }
    }
}

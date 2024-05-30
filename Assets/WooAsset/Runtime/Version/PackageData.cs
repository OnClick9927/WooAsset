using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class PackageData : IBufferObject
    {
        public string name;
        public string description;
        public string manifestFileName => VersionHelper.GetManifestFileName(name);
        public string bundleFileName => VersionHelper.GetBundleFileName(name);

        public List<string> tags = new List<string>();

        void IBufferObject.ReadData(BufferReader reader)
        {
            name = reader.ReadUTF8();
            description = reader.ReadUTF8();
            tags = reader.ReadUTF8List();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8(name);
            writer.WriteUTF8(description);
            writer.WriteUTF8List(tags);
        }
    }
}

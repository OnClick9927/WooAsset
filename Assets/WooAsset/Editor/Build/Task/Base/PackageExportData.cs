using static WooAsset.AssetsBuildOption;

namespace WooAsset
{
    [System.Serializable]
    public class PackageExportData : IBufferObject
    {
        public string encrypt;
        public string version;
        public string compress;
        public TypeTreeOption typeTreeOption;
        public PackageData pkg;
        public ManifestData manifest;

        void IBufferObject.ReadData(BufferReader reader)
        {
            encrypt = reader.ReadUTF8();
            version = reader.ReadUTF8();
            compress = reader.ReadUTF8();
            typeTreeOption = (TypeTreeOption)reader.ReadUInt16();
            pkg = reader.ReadObject<PackageData>();
            manifest = reader.ReadObject<ManifestData>();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8(encrypt);
            writer.WriteUTF8(version);
            writer.WriteUTF8(compress);
            writer.WriteUInt16((ushort)typeTreeOption);
            writer.WriteObject(pkg);
            writer.WriteObject(manifest);
        }
    }
}

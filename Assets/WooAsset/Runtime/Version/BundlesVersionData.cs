
using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class BundlesVersionData : IBufferObject
    {

        public string version;
        public List<BundleFileData> bundles = new List<BundleFileData>();

        void IBufferObject.ReadData(BufferReader reader)
        {
            version = reader.ReadUTF8();
            bundles = reader.ReadObjectList<BundleFileData>();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8(version);
            writer.WriteObjectList(bundles);
        }
    }

}

namespace WooAsset
{
    public class NoneAssetStreamEncrypt : IAssetStreamEncrypt
    {
        public byte[] Decode(string bundleName, byte[] buffer)
        {
            return buffer;
        }

        public byte[] Encode(string bundleName, byte[] buffer)
        {
            return buffer;
        }
    }
}

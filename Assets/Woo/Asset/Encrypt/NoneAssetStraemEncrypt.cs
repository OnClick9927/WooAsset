namespace WooAsset
{
    public class NoneAssetStraemEncrypt : IAssetStraemEncrypt
    {
        public byte[] DeCode(string bundleName, byte[] buffer)
        {
            return buffer;
        }

        public byte[] EnCode(string bundleName, byte[] buffer)
        {
            return buffer;
        }
    }
}

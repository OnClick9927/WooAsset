namespace WooAsset
{
    public class NoneAssetStreamEncrypt : IAssetStreamEncrypt
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

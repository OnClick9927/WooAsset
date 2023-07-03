namespace WooAsset
{
    public class EncryptBuffer
    {
        public static byte[] Encode(string bundleName, byte[] buffer, IAssetStreamEncrypt en)
        {
            return en.Encode(bundleName, buffer);
        }
        public static byte[] Decode(string bundleName, byte[] buffer, IAssetStreamEncrypt en)
        {
            return en.Decode(bundleName, buffer);
        }
    }
}

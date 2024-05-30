namespace WooAsset
{
    public class EncryptBuffer
    {
        public static byte[] Encode(string bundleName, byte[] buffer, IAssetStreamEncrypt en)
        {
            return en.Encode(bundleName, buffer);
        }
        public static byte[] Decode(string bundleName, byte[] buffer, int offset, int length, IAssetStreamEncrypt en)
        {
            return en.Decode(bundleName, buffer, offset, length);
        }
        public static byte[] Decode(string bundleName, byte[] buffer, IAssetStreamEncrypt en)
        {
            return en.Decode(bundleName, buffer);
        }
    }
}

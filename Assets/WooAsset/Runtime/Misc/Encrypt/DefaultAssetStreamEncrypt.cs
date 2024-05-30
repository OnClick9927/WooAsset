namespace WooAsset
{
    public class DefaultAssetStreamEncrypt : IAssetStreamEncrypt
    {
        public const int code = 1;
        public byte[] Decode(string bundleName, byte[] buffer)
        {
            return Decode(bundleName, buffer, 0, buffer.Length);
        }

        public byte[] Decode(string bundleName, byte[] buffer, int offset, int length)
        {
            for (int i = offset; i < offset + length && i < buffer.Length; i++)
            {
                buffer[i] ^= (byte)i;
            }
            return buffer;
        }

        public byte[] Encode(string bundleName, byte[] buffer)
        {
            return Decode(bundleName, buffer);
        }
    }
}

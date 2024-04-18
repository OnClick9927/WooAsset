using UnityEngine;

namespace WooAsset
{
    public class DefaultAssetStreamEncrypt : IAssetStreamEncrypt
    {
        public byte[] Decode(string bundleName, byte[] buffer)
        {
            return Decode(bundleName, buffer, 0, buffer.Length);
        }

        public byte[] Decode(string bundleName, byte[] buffer, int offset, int length)
        {
            for (int i = 0; i < offset + length && i < buffer.Length; i++)
            {
                byte key = (byte)bundleName[i % bundleName.Length];
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

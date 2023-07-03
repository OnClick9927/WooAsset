using UnityEngine;

namespace WooAsset
{
    public class DefaultAssetStreamEncrypt : IAssetStreamEncrypt
    {
        public byte[] Decode(string bundleName, byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                byte key = (byte)bundleName[(int)Mathf.Repeat(i, bundleName.Length)];
                buffer[i] ^= key;
            }
            return buffer;
        }

        public byte[] Encode(string bundleName, byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                byte key = (byte)bundleName[(int)Mathf.Repeat(i, bundleName.Length)];
                buffer[i] ^= key;
            }
            return buffer;

        }
    }
}

using System;
using UnityEngine;

namespace WooAsset
{
    public class ReverseAssetStreamEncrypt : IAssetStreamEncrypt
    {
        public byte[] Decode(string bundleName, byte[] buffer)
        {
            int total = 0;
            int len = bundleName.Length;
            for (int i = 0; i < len; i++)
                total += bundleName[i];
            int once = total / len;
            int offset = 0;
            int last = buffer.Length;
            while (last > 0)
            {
                int read = Mathf.Min(last, once);
                Array.Reverse(buffer, offset, read);
                last -= read;
                offset += read;
            }
            return buffer;
        }

        public byte[] Encode(string bundleName, byte[] buffer)
        {
            return Decode(bundleName, buffer);
        }
    }
}

using System;

namespace WooAsset
{
    public interface IAssetEncrypt
    {
        byte[] Encode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer, int offset, int length);


    }
    public class NoneAssetStreamEncrypt : IAssetEncrypt
    {
        public const int code = 0;

        byte[] IAssetEncrypt.Decode(string bundleName, byte[] buffer)
        {
            return buffer;
        }

        byte[] IAssetEncrypt.Decode(string bundleName, byte[] buffer, int offset, int length)
        {
            return buffer;
        }

        byte[] IAssetEncrypt.Encode(string bundleName, byte[] buffer)
        {
            return buffer;
        }
    }
    public class DefaultAssetStreamEncrypt : IAssetEncrypt
    {
        public const int code = 1;
        byte[] IAssetEncrypt.Decode(string bundleName, byte[] buffer)
        {
            return ((IAssetEncrypt)this).Decode(bundleName, buffer, 0, buffer.Length);
        }

        byte[] IAssetEncrypt.Decode(string bundleName, byte[] buffer, int offset, int length)
        {
            for (int i = offset; i < offset + length && i < buffer.Length; i++)
            {
                buffer[i] ^= (byte)i;
            }
            return buffer;
        }

        byte[] IAssetEncrypt.Encode(string bundleName, byte[] buffer)
        {
            return ((IAssetEncrypt)this).Decode(bundleName, buffer);
        }
    }

    public class OffsetAssetStreamEncrypt : IAssetEncrypt
    {
        public const int code = 3;

        public virtual ulong GetOffset(string bundleName) => 8;
        public byte[] Decode(string bundleName, byte[] buffer)
        {
            ulong offset = GetOffset(bundleName);
            Array.Copy(buffer, (int)offset, buffer, 0, buffer.Length - (int)offset);
            return buffer;
        }

        public byte[] Decode(string bundleName, byte[] buffer, int offset, int length)
        {
            return null;
        }

        public byte[] Encode(string bundleName, byte[] buffer)
        {
            ulong offset = GetOffset(bundleName);
            byte[] tmp = new byte[(int)offset + buffer.Length];
            for (int i = 0; i < (int)offset; i++)
                tmp[i] = (byte)bundleName[i % bundleName.Length];
            Array.Copy(buffer, 0, tmp, (int)offset, buffer.Length);
            return tmp;
        }
    }
}

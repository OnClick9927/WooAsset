namespace WooAsset
{
    public interface IAssetEncrypt
    {
        byte[] Encode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer, int offset, int length);


    }
    public class EncryptBuffer
    {
        public static byte[] Encode(string bundleName, byte[] buffer, IAssetEncrypt en)
        {
            return en.Encode(bundleName, buffer);
        }
        public static byte[] Decode(string bundleName, byte[] buffer, int offset, int length, IAssetEncrypt en)
        {
            return en.Decode(bundleName, buffer, offset, length);
        }
        public static byte[] Decode(string bundleName, byte[] buffer, IAssetEncrypt en)
        {
            return en.Decode(bundleName, buffer);
        }
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
}

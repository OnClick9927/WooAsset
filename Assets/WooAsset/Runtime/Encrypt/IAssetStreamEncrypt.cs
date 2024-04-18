namespace WooAsset
{
    public interface IAssetStreamEncrypt
    {
        byte[] Encode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer, int offset, int length);


    }
}

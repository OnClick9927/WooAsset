namespace WooAsset
{
    public interface IAssetStreamEncrypt
    {
        byte[] EnCode(string bundleName, byte[] buffer);
        byte[] DeCode(string bundleName, byte[] buffer);
    }
}

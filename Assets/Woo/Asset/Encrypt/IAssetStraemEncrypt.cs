namespace WooAsset
{
    public interface IAssetStraemEncrypt
    {
        byte[] EnCode(string bundleName, byte[] buffer);
        byte[] DeCode(string bundleName, byte[] buffer);
    }
}

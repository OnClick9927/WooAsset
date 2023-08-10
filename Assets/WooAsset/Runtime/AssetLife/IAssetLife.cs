

namespace WooAsset
{
    public interface IAssetLife
    {
    }
    public interface IAssetLife<T>: IAssetLife where T : IAsset
    {
        void OnAssetCreate(string path, T asset);
        void OnAssetRetain(T asset, int count);
        void OnAssetRelease(T asset, int count);
        void OnAssetUnload(string path, T asset);
    }
}

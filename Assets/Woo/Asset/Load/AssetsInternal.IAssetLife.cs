

namespace WooAsset
{
    partial class AssetsInternal
    {
        public interface IAssetLife<T> where T : IAsset
        {
            void OnAssetCreate(string path, T asset);
            void OnAssetRetain(T asset, int count);
            void OnAssetRelease(T asset, int count);
            void OnAssetUnload(string path, T asset);
        }
    }
}

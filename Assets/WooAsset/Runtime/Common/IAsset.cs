

namespace WooAsset
{
    public interface IAsset
    {
        int refCount { get; }
        void UnLoad();
        void LoadAsync();
        void Retain();

        int Release();

    }
}



namespace WooAsset
{
    public interface IAsset
    {
        int refCount { get; }
        bool unloaded { get; }
        void UnLoad();
        void LoadAsync();
        void Retain();

        int Release();

    }
}

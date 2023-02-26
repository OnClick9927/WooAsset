
namespace WooAsset
{
    public abstract class Asset<T> : AssetOperation, IAsset
    {
        public T value { get; private set; }

        public abstract string path { get; }
        void IAsset.LoadAsync()
        {
            OnLoad();
        }
        void IAsset.UnLoad()
        {
            OnUnLoad();
        }

        protected abstract void OnUnLoad();
        protected abstract void OnLoad();

        protected void SetResult(T value)
        {
            this.value = value;
            InvokeComplete();
        }

    }
}

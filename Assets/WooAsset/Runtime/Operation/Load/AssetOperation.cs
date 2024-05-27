
using System;

namespace WooAsset
{
    public abstract class AssetOperation<T> : AssetOperation
    {
        public T value { get; private set; }
        protected virtual void SetResult(T value)
        {
            this.value = value;
            InvokeComplete();
        }
    }
    public abstract class AssetOperation : Operation, IAsset
    {

        public abstract bool async { get; }
        public DateTime time { get; private set; }

        private int _ref;
        public int refCount => _ref;

        void IAsset.LoadAsync()
        {
            time = DateTime.Now;
            OnLoad();
        }
        void IAsset.UnLoad()
        {
            OnUnLoad();
        }

        protected abstract void OnUnLoad();
        protected abstract void OnLoad();


        void IAsset.Retain()
        {
            _ref++;
        }
        int IAsset.Release()
        {
            _ref--;
            return _ref;
        }
    }
}

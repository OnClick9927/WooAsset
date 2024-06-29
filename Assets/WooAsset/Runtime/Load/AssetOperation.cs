
using System;

namespace WooAsset
{
    interface IAsset
    {
        int refCount { get; }
        void UnLoad();
        void LoadAsync();
        void Retain();

        int Release();

    }

    public abstract class AssetOperation : Operation, IAsset
    {

        public abstract bool async { get; }
        public DateTime time { get; private set; }

        private int _ref;
        public int refCount => _ref;

        void IAsset.Retain() => _ref++;

        int IAsset.Release()
        {
            _ref--;
            return _ref;
        }
        void IAsset.LoadAsync()
        {
            time = DateTime.Now;
            OnLoad();
        }
        void IAsset.UnLoad() => OnUnLoad();

        protected abstract void OnUnLoad();
        protected abstract void OnLoad();

    }
}

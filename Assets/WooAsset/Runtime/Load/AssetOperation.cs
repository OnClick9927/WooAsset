
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

    public abstract class AssetOperation : LoopOperation, IAsset
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
            Update();
        }
        void IAsset.UnLoad() => OnUnLoad();
        protected sealed override bool NeedUpdate() => base.NeedUpdate();
        protected abstract void OnUnLoad();
        protected abstract void OnLoad();

        protected sealed override void AddToLoop() => base.AddToLoop();
        protected sealed override void OnUpdate() => OnLoad();

    }
}

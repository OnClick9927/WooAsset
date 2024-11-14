
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

        private enum State
        {
            None, Load, UnLoad
        }
        private State _state;
        int IAsset.Release()
        {
            _ref--;
            return _ref;
        }
        void IAsset.LoadAsync()
        {
            _state = State.Load;
            time = DateTime.Now;
            //OnLoad();
        }
        void IAsset.UnLoad()
        {
            _state = State.UnLoad;
        }
        protected sealed override bool NeedUpdate()
        {
            return _state != State.None;
        }
        protected abstract void OnUnLoad();
        protected abstract void OnLoad();

        protected sealed override void AddToLoop()
        {
            base.AddToLoop();
        }
        protected sealed override void OnUpdate()
        {
            if (_state == State.Load)
            {
                OnLoad();
                if (isDone)
                    _state = State.None;
            }
            else if (_state == State.UnLoad)
            {
                OnUnLoad();
                _state = State.None;
            }

        }

    }
}



using System;

namespace WooAsset
{
    public abstract class AssetOperation 
    {
        public bool isDone { get; private set; }

        public abstract float progress { get; }

        private string _err;
        public string error { get { return _err; } }

        public event Action completed;

        protected void InvokeComplete()
        {
            isDone = true;
            completed?.Invoke();
        }
        protected void SetErr(string err)
        {
            _err = err;
            AssetsInternal.LogError(err);
        }

        public void WaitForComplete()
        {
            while (!isDone)
            {

            }
        }
    }
}



using System;
using System.Collections;

namespace WooAsset
{
    public abstract class AssetOperation : IEnumerator
    {
        public bool isDone { get; private set; }

        public abstract float progress { get; }

        private string _err;
        public string error { get { return _err; } }
        public bool isErr { get { return !string.IsNullOrEmpty(error); } }

        public event Action completed;

        protected void InvokeComplete()
        {
            isDone = true;
            completed?.Invoke();
        }
        protected void SetErr(string err)
        {
            _err = err;
            AssetsHelper.LogError(this.error);
        }
        bool IEnumerator.MoveNext() => !isDone;
        void IEnumerator.Reset()
        {
            isDone = false;
            completed = null;
            _err = String.Empty;
        }
        object IEnumerator.Current => this;

        public void WaitForComplete()
        {
            while (!isDone)
            {

            }
        }
    }
}

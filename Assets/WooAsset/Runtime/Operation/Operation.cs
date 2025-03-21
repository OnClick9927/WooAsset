using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace WooAsset
{
    public abstract class Operation : IEnumerator
    {
        private static Operation _empty = new EmptyOperation();
        public static Operation empty { get { return _empty; } }

        private bool _isDone;
        public bool isDone => _isDone;

        public abstract float progress { get; }

        private string _err;
        public string error { get { return _err; }protected set { _err = value; } }
        public bool isErr { get { return !string.IsNullOrEmpty(error); } }

        public event Action<Operation> completed;

        protected void InvokeComplete()
        {
            if (_isDone)
                return;
            _isDone = true;
            completed?.Invoke(this);
        }
        protected void SetErr(string err)
        {
            _err = err;
            AssetsHelper.LogError(this.error);
        }
        bool IEnumerator.MoveNext() => !_isDone;
        void IEnumerator.Reset()
        {
            _isDone = false;
            completed = null;
            _err = String.Empty;
        }
        object IEnumerator.Current => _isDone ? this : null;

        public async void WaitForComplete()
        {
            while (!isDone)
            {
                await new YieldOperation();
            }
        }
    }

    class EmptyOperation : Operation
    {
        public override float progress => 1;

        public EmptyOperation()
        {
            InvokeComplete();
        }
    }

    public class GroupOperation<T> : Operation where T : Operation
    {
        public override float progress => isDone ? 1 : _count == 0 ? 0 : (float)_index / _count;
        private int _count;
        private int _index;
        public int count => _count;
        public void Done(List<T> ops)
        {
            if (ops != null && ops.Count != 0)
            {
                _count = ops.Count;
                for (int i = 0; i < ops.Count; i++)
                {
                    var op = ops[i];
                    if (op.isDone)
                        _index++;
                    else
                        op.completed += Op_completed;
                }
            }
            CheckComplete();
        }
        protected virtual void BeforeInvokeComplete() { }
        private void CheckComplete()
        {
            if (_index >= _count)
                InvokeComplete();
        }
        public new void InvokeComplete()
        {
            if (isDone) return;
            BeforeInvokeComplete();
            base.InvokeComplete();
        }
        private void Op_completed(Operation operation)
        {
            _index++;
            //AssetsHelper.LogError(progress.ToString());
            operation.completed -= Op_completed;
            if (operation.isErr)
                SetErr(operation.error);
            CheckComplete();
        }
    }



    public abstract class LoopOperation : Operation
    {
        protected virtual void AddToLoop()
        {
            AssetsLoop.AddOperation(this);
        }
        protected LoopOperation()
        {
            AddToLoop();
        }
        protected virtual bool NeedUpdate() { return !isDone; }
        public void Update()
        {
            if (NeedUpdate()) OnUpdate();
        }
        protected abstract void OnUpdate();
    }
    public class YieldOperation : LoopOperation
    {
        public override float progress => isDone ? 1 : 0;

        protected virtual async void EditorWait()
        {
#if UNITY_EDITOR
            await System.Threading.Tasks.Task.Delay(1);
            InvokeComplete();
#endif
        }
        protected override void AddToLoop()
        {
#if UNITY_EDITOR
            EditorWait();
#else
            base.AddToLoop();

#endif
        }
        protected override void OnUpdate()
        {
            InvokeComplete();
        }
    }

    class WaitBusyOperation : YieldOperation
    {
        protected async override void EditorWait()
        {
#if UNITY_EDITOR
            while (AssetsLoop.isBusy)
                await System.Threading.Tasks.Task.Delay(10);
            InvokeComplete();
#endif
        }
        protected override void OnUpdate()
        {
            if (!AssetsLoop.isBusy)
                InvokeComplete();
        }
    }
    public class ReadFileOperation : Operation
    {
        private int n;
        private string path;
        public byte[] bytes;
        private bool async;

        public override float progress => _progress;
        private float _progress;
        public ReadFileOperation(string path, bool async, int n = 1024 * 1024)
        {
            this.n = n;
            this.path = path;
            this.async = async;
            Done();
        }
        private async void Done()
        {
            if (async)
            {
                int offset = 0;
                using (FileStream fs = File.OpenRead(path))
                {
                    long len = fs.Length;
                    bytes = new byte[len];
                    long last = len;
                    while (last > 0)
                    {
                        var read = fs.Read(bytes, offset, (int)Math.Min(n, last));
                        offset += read;
                        last -= read;
                        _progress = offset / (float)len;
                        if (last <= 0) break;
                        await new YieldOperation();
                    }
                }
            }
            else
            {
                bytes = File.ReadAllBytes(path);
            }
            InvokeComplete();
        }
    }
    class WriteFileOperation : Operation
    {
        private int n;
        private string targetPath;
        public override float progress => isDone ? 1 : _progress;
        private float _progress;
        public WriteFileOperation(string targetPath, byte[] bytes, int start, int len, int n = 1024 * 1024)
        {
            this.n = n;
            this.targetPath = targetPath;
            CopyFromBytes(bytes, start, len);
        }
        private async void CopyFromBytes(byte[] bytes, int start, int _len)
        {
            int offset = start;
            long len = _len;
            long last = len;
            using (FileStream _fs = File.OpenWrite(targetPath))
            {
                _fs.Seek(0, SeekOrigin.Begin);

                while (last > 0)
                {
                    var read = (int)Math.Min(n, last);
                    _fs.Write(bytes, offset, read);
                    offset += read;
                    last -= read;
                    _progress = offset / (float)len;
                    if (last <= 0) break;

                    await new YieldOperation();
                }
            }

            InvokeComplete();
        }

    }

}
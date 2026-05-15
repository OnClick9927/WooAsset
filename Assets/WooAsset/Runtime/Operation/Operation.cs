using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace WooAsset
{
    public enum ExceptionType
    {
        Unknown,
        IO,
        Bundle,
        Instantiate,
        DownLoad,
        LoadManifestOperation,
        Editor
    }
    public class OperationException
    {
        //public OperationException() : base() { }
        //public OperationException(string message) : base(message) { }
        //public OperationException(string message, Exception inner) : base(message, inner) { }
        public Enum code { get; private set; }
        public string message { get; private set; }
        public ExceptionType type { get; private set; }
        public Exception exception { get; private set; }

        public static OperationException Create(ExceptionType exceptionType, Enum code, string msg = "")
        {
            var ex = new OperationException();
            ex.type = exceptionType;
            ex.message = msg;
            ex.code = code;
            return ex;
        }
        public static OperationException CreateUnknown(ExceptionType exceptionType, Exception inner)
        {
            var ex = new OperationException();
            ex.type = exceptionType;
            ex.message = inner.Message;
            ex.exception = inner;
            ex.code = ExceptionType.Unknown;
            return ex;
        }
    }

    public abstract class Operation : IEnumerator
    {
        private static Operation _empty = new EmptyOperation();
        private static YieldOperation _yield = new YieldOperation();
        private static WaitBusyOperation _busy = new WaitBusyOperation();

        public static Operation empty { get { return _empty; } }
        public static Operation yield
        {
            get
            {
                //if (_yield.isDone)
                //{
                //    //_yield.ResetIsDone();
                //}
                return _yield.Begin();
            }
        }
        internal static Operation busy
        {
            get
            {
                if (_busy.isDone)
                {
                    _busy.ResetIsDone();
                }
                return _busy.Begin();
            }
        }

        private bool _isDone;
        public bool isDone => _isDone;

        public abstract float progress { get; }



        private OperationException _err;
        public OperationException error { get { return _err; } protected set { _err = value; } }

        public bool isErr => _err != null;

        public event Action<Operation> completed;
        protected void InvokeComplete()
        {
            if (_isDone)
                return;
            _isDone = true;
            completed?.Invoke(this);
        }
        protected void SetErr(OperationException ex)
        {
            _err = ex;
            if (ex.exception != null)
            {
                throw ex.exception;
            }
            else
            {
                string _msg = $"{this.GetType().Name}_{ex.type}_{ex.code}:{ex.message}";
                if (ex.type == ExceptionType.Editor)
                    throw new Exception(_msg);
                else
                    AssetsHelper.LogError(_msg);
            }
        }
        bool IEnumerator.MoveNext() => !_isDone;
        void IEnumerator.Reset()
        {
            _isDone = false;
            completed = null;
            _err = null;
        }
        object IEnumerator.Current => _isDone ? this : null;

        public async void WaitForComplete()
        {
            while (!isDone)
            {
                await Operation.yield;
            }
        }

        internal virtual void ResetIsDone()
        {
            completed = null;
            _isDone = false;
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
                        Op_completed(op);
                    else
                        op.completed += Op_completed;
                }
            }
            else
            {
                InvokeComplete();
            }
        }
        protected virtual void BeforeInvokeComplete() { }
        public new void InvokeComplete()
        {
            if (isDone) return;
            BeforeInvokeComplete();
            base.InvokeComplete();
        }
        private void Op_completed(Operation operation)
        {
            _index++;
            operation.completed -= Op_completed;
            if (operation.isErr)
                SetErr(operation.error);
            if (_index >= _count)
                InvokeComplete();
        }
    }



    abstract class LoopOperation : Operation
    {
        bool working;
        public Operation Begin()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                if (!working)

                    UnityEditor.EditorApplication.update += OnUpdate;
                return this;
            }
#endif
            if (!working)
            {
                AssetsLoop.instance.update += OnUpdate;
                working = true;
                OnBegin();
            }
            return this;
        }
        protected virtual void OnBegin() { }
        protected new void InvokeComplete()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.update -= OnUpdate;
            }
#endif
            AssetsLoop.instance.update -= OnUpdate;
            working = false;

            base.InvokeComplete();
        }
        protected abstract void OnUpdate();
    }
    class YieldOperation : LoopOperation
    {
        public override float progress => isDone ? 1 : 0;
        int index;

        protected override void OnUpdate()
        {
            if (index++ >= 1)
            {
                InvokeComplete();
                index = 0;

                base.ResetIsDone();

            }
        }

    }

    class WaitBusyOperation : LoopOperation
    {
        public override float progress => isDone ? 1 : 0;
        protected override void OnUpdate()
        {
            if (!AssetsLoop.instance.isBusy)
                InvokeComplete();
        }
    }


    public class ReadFileOperation : Operation
    {

        public enum Errcode
        {
            FileNotExist,
        }
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
            if (!AssetsHelper.ExistsFile(this.path))
            {
                SetErr(OperationException.Create(ExceptionType.IO, Errcode.FileNotExist));
                InvokeComplete();
            }
            else
            {
                try
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
                                await Operation.yield;
                            }
                        }
                    }
                    else
                    {
                        bytes = File.ReadAllBytes(path);
                    }
                }
                catch (Exception ex)
                {
                    SetErr(OperationException.CreateUnknown(ExceptionType.IO, ex));

                }
                finally
                {
                    InvokeComplete();
                }
            }
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
            try
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

                        await Operation.yield;
                    }
                }

            }
            catch (Exception ex)
            {
                SetErr(OperationException.CreateUnknown(ExceptionType.IO, ex));

            }
            finally
            {
                InvokeComplete();
            }
        }

    }

}
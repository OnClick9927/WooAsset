

using System;
using System.Collections;
using System.Collections.Generic;
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
        public string error { get { return _err; } }
        public bool isErr { get { return !string.IsNullOrEmpty(error); } }

        public event Action<Operation> completed;

        protected void InvokeComplete()
        {
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
        public override float progress => isDone ? 1 : (float)_index / _count;
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
                    var bundle = ops[i];
                    if (bundle.isDone)
                        _index++;
                    else
                    {
                        bundle.completed += Op_completed;
                    }
                }
            }
            CheckComplete();

        }
        protected virtual void BeforeInvokeComplete()
        {

        }
        private void CheckComplete()
        {
            if (_index >= _count)
            {
                BeforeInvokeComplete();
                InvokeComplete();
            }
        }
        private void Op_completed(Operation operation)
        {
            _index++;
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

    public class WaitBusyOperation : YieldOperation
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

}

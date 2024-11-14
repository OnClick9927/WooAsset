

using System;
using System.Collections;
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

        public event Action completed;

        protected void InvokeComplete()
        {
            _isDone = true;
            completed?.Invoke();
            completed = null;
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

    public class GroupOperation : Operation
    {
        private readonly Operation[] ops;
        private int total;
        private int step;
        public override float progress => isDone ? 1 : step / (float)total;

        public GroupOperation(Operation[] ops)
        {
            this.ops = ops;
            if (this.ops == null || this.ops.Length == 0)
                InvokeComplete();
            else
                Done();
        }

        private async void Done()
        {
            total = this.ops.Length;
            for (int i = 0; i < total; i++)
            {
                await this.ops[i];
                step = i;
            }
            InvokeComplete();
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


}

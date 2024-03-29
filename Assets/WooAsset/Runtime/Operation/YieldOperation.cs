﻿using System.Threading.Tasks;

namespace WooAsset
{
    public class YieldOperation : Operation
    {
        public override float progress => isDone ? 1 : 0;
        public YieldOperation()
        {
#if UNITY_EDITOR
            EditorWait();
#else
            AssetsLoop.AddOperation(this);
#endif
        }
        protected virtual async void EditorWait()
        {
#if UNITY_EDITOR
            await Task.Delay(1);
            InvokeComplete();
#endif
        }
        public virtual void NormalLoop()
        {
            InvokeComplete();
        }
    }

}

using System.Collections;
using System.Collections.Generic;

namespace WooAsset
{
    public abstract class AssetTask : Operation
    {
        public override float progress => 1;
        public static AssetTask Execute(List<AssetTask> tasks, AssetTaskParams param)
        {
            return new AssetTaskRunner(tasks).Execute(new AssetTaskContext(param));
        }
        public static AssetTask Execute(AssetTask task, AssetTaskContext context)
        {
            return task.Execute(context);
        }
        protected AssetTask Execute(AssetTaskContext context)
        {
            (this as IEnumerator).Reset();
            this.context = context;
            OnExecute(context);
            return this;
        }
        protected AssetTaskContext context;
        protected abstract void OnExecute(AssetTaskContext context);

        protected new void SetErr(string err)
        {
            base.error = err;

        }

    }
}

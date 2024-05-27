using System.Collections;

namespace WooAsset
{
    public abstract class AssetTask : Operation
    {
        public override float progress => 1;
        protected static AssetTask Execute(AssetTask task, AssetTaskContext context)
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

    }
}

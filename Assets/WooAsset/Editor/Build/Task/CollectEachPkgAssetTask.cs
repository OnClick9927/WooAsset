using System.Collections.Generic;

namespace WooAsset
{
    public class CollectEachPkgAssetTask : AssetTask
    {

        private List<AssetTask> tasks = new List<AssetTask>()
        {
            new CollectAssetsTask(),
        };


        protected override async void OnExecute(AssetTaskContext context)
        {
            var builds = context.buildPkgs;
            if (builds.Count == 0)
            {
                SetErr("Nothing To Build");
                InvokeComplete();
                return;
            }

            for (int i = 0; i < builds.Count; i++)
            {
                var group = builds[i];
                context.buildPkg = group;
                for (int j = 0; j < tasks.Count; j++)
                    await Execute(tasks[j], context);
            }
            InvokeComplete();
        }
    }
}

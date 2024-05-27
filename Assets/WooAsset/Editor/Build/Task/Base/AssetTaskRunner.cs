using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;

namespace WooAsset
{
    public class AssetTaskRunner : AssetTask
    {
        class SegmentOperation : Operation
        {
            private List<AssetTask> tasks;
            private AssetTaskRunner context;

            public SegmentOperation(List<AssetTask> tasks, AssetTaskRunner context)
            {
                this.tasks = tasks;
                this.context = context;
                Done();
            }

            public override float progress => _progress;
            private float _progress = 0;
            private async void Done()
            {
                if (tasks != null)
                {
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        _progress = (float)i / tasks.Count;
                        await Task.Delay(100);
                        await Execute(tasks[i], context.context);
                        if (tasks[i].isErr)
                        {
                            SetErr($"{context.context.Pipeline} Err");
                            break;
                        }
                    }
                }
                InvokeComplete();
            }
        }

        protected AssetTaskRunner(List<AssetTask> tasks)
        {
            this.tasks = tasks;
        }
        private List<AssetTask> tasks = new List<AssetTask>();

        public override float progress => isDone ? 1 : 0;


        protected async override void OnExecute(AssetTaskContext context)
        {
            Stopwatch sw = Stopwatch.StartNew();
            await Task.Delay(100);
            var op = await new SegmentOperation(context.pipelineStartTasks, this);
            if (!op.isErr)
            {
                op = await new SegmentOperation(tasks, this);
                if (!op.isErr)
                    await new SegmentOperation(context.pipelineEndTasks, this);
            }

            sw.Stop();
            InvokeComplete();
            AssetsHelper.Log($"{context.Pipeline} Finish {sw.Elapsed.ToString(@"G")}");
        }




        [MenuItem(TaskPipelineMenu.PreviewBundles)]
        public static AssetTask PreviewBundles()
        {
            AssetTaskContext context = new AssetTaskContext()
            {
                Pipeline = TaskPipelineType.PreviewBundles,
            };
            AssetTask task = Execute(new AssetTaskRunner(hashPreview), context);
            return task;
        }
        [MenuItem(TaskPipelineMenu.PreviewAssets)]
        public static AssetTask PreviewAssets()
        {
            AssetTaskContext context = new AssetTaskContext()
            {
                Pipeline = TaskPipelineType.PreviewAssets,
            };
            AssetTask task = Execute(new AssetTaskRunner(collectAssets), context);
            return task;
        }

        [MenuItem(TaskPipelineMenu.PreviewAllBundles)]
        public static AssetTask PreviewAllBundles()
        {
            AssetTaskContext context = new AssetTaskContext()
            {
                Pipeline = TaskPipelineType.PreviewAllBundles,
            };
            AssetTask task = Execute(new AssetTaskRunner(hashPreview), context);
            return task;
        }
        [MenuItem(TaskPipelineMenu.PreviewAllAssets)]
        public static AssetTask PreviewAllAssets()
        {
            AssetTaskContext context = new AssetTaskContext()
            {
                Pipeline = TaskPipelineType.PreviewAllAssets,
            };
            AssetTask task = Execute(new AssetTaskRunner(collectAssets), context);
            return task;
        }




        [MenuItem(TaskPipelineMenu.Build)]
        public static AssetTask Build()
        {
            AssetTaskContext context = new AssetTaskContext() { Pipeline = TaskPipelineType.BuildBundle };
            AssetTask task = Execute(new AssetTaskRunner(stream_common), context);
            return task;
        }
  
    
        private static List<AssetTask> stream_common = new List<AssetTask>
        {
            new PrepareTask(),
            new BuildBundleTask(),
            new CopyToBundlesToServerTask(),
            new CopyBundlesToStreamTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };





        private static List<AssetTask> collectAssets = new List<AssetTask>
        {
            new PrepareTask(),
            new CollectAssetsTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> hashPreview = new List<AssetTask>
        {
            new PrepareTask(),
            new CollectAssetsTask(),
            new CollectHashBundleGroupTask(),
            new FastModeManifestTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };





    }
}

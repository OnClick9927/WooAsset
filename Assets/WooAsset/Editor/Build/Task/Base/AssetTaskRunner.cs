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


        [MenuItem(TaskPipelineMenu.EditorSimulate)]
        public static AssetTask EditorSimulate() => EditorSimulate(false);
        public static AssetTask EditorSimulate(bool fuzzySearch)
        {
            var Params = new AssetTaskParams(TaskPipelineType.EditorSimulate) { fuzzySearch = fuzzySearch };

            AssetTask task = Execute(new AssetTaskRunner(stream_common), Params);
            return task;
        }


        [MenuItem(TaskPipelineMenu.PreviewAssets)]
        public static AssetTask PreviewAssets()
        {
            var Params = new AssetTaskParams(TaskPipelineType.PreviewAssets);
            AssetTask task = Execute(new AssetTaskRunner(stream_common), Params);
            return task;
        }

        [MenuItem(TaskPipelineMenu.PreviewAllAssets)]
        public static AssetTask PreviewAllAssets()
        {
            var Params = new AssetTaskParams(TaskPipelineType.PreviewAllAssets);
            AssetTask task = Execute(new AssetTaskRunner(collectAssets), Params);
            return task;
        }

        [MenuItem(TaskPipelineMenu.Build)]
        public static AssetTask Build()
        {
            var Params = new AssetTaskParams(TaskPipelineType.BuildBundle);
            AssetTask task = Execute(new AssetTaskRunner(stream_common), Params);
            return task;
        }
        [MenuItem(TaskPipelineMenu.DryBuild)]
        public static AssetTask DryBuild()
        {
            var Params = new AssetTaskParams(TaskPipelineType.DryBuild);
            AssetTask task = Execute(new AssetTaskRunner(stream_common), Params);
            return task;
        }

        private static List<AssetTask> stream_common = new List<AssetTask>
        {
            new PrepareTask(),
            new BuildBundleTask(),
            new CopyToBundlesToServerTask(),
            new CopyBundlesToStreamTask(),
            new BuildExportTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> collectAssets = new List<AssetTask>
        {
            new PrepareTask(),
            new CollectAssetsTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };



    }
}

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
        public static AssetTaskRunner PreviewBundles()
        {
            AssetTaskContext context = new AssetTaskContext() { Pipeline = TaskPipelineType.PreviewBundles };
            AssetTaskRunner task = Execute(new AssetTaskRunner(hashPreview), context) as AssetTaskRunner;
            return task;
        }
        [MenuItem(TaskPipelineMenu.PreviewAssets)]
        public static AssetTask PreviewAssets()
        {
            AssetTaskContext context = new AssetTaskContext() { Pipeline = TaskPipelineType.PreviewAssets };
            AssetTask task = Execute(new AssetTaskRunner(collectAssets), context);
            return task;
        }
        [MenuItem(TaskPipelineMenu.Build)]
        public static AssetTask Build()
        {
            AssetTaskContext context = new AssetTaskContext() { Pipeline = TaskPipelineType.BuildBundle };
            AssetTask task = Execute(new AssetTaskRunner(common), context);
            return task;
        }
        [MenuItem(TaskPipelineMenu.BuildToStream)]
        public static AssetTask BuildToStream()
        {
            AssetTaskContext context = new AssetTaskContext() { Pipeline = TaskPipelineType.BuildBundle };
            AssetTask task = Execute(new AssetTaskRunner(stream_common), context);
            return task;
        }
        [MenuItem(TaskPipelineMenu.ShaderVariant)]
        public static AssetTask ShaderVariant()
        {
            return Execute(new AssetTaskRunner(shader), new AssetTaskContext() { Pipeline = TaskPipelineType.ShaderVariant });
        }
        [MenuItem(TaskPipelineMenu.SpriteAtlas)]
        public static AssetTask SpriteAtlas()
        {
            return Execute(new AssetTaskRunner(sprite), new AssetTaskContext() { Pipeline = TaskPipelineType.SpriteAtlas });
        }
        [MenuItem(TaskPipelineMenu.ClearHistory)]
        public static AssetTask ClearHistory()
        {
            return Execute(new AssetTaskRunner(clearHistory), new AssetTaskContext() { Pipeline = TaskPipelineType.SpriteAtlas });
        }
        [MenuItem(TaskPipelineMenu.ClearOutput)]
        public static AssetTask ClearOutput()
        {
            return Execute(new AssetTaskRunner(clearOutput), new AssetTaskContext() { Pipeline = TaskPipelineType.ClearOutput });
        }
        [MenuItem(TaskPipelineMenu.OpenOutput)]
        public static AssetTask OpenOutput()
        {
            return Execute(new AssetTaskRunner(openOutput), new AssetTaskContext() { Pipeline = TaskPipelineType.OpenOutput });
        }

        private static List<AssetTask> stream_common = new List<AssetTask>
        {
            new PrepareTask(),
            new RawAssetTask(),
            new ShaderVariantTask(),
            new SpriteAtlasTask(),
            new BuildBundleTask(),
            new BuildExportTask(),
            new CopyToBundlesToServerTask(),
            new CopyBundlesToStreamTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> common = new List<AssetTask>
        {
            new PrepareTask(),
            new RawAssetTask(),
            new ShaderVariantTask(),
            new SpriteAtlasTask(),
            new BuildBundleTask(),
            new BuildExportTask(),
            new CopyToBundlesToServerTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };



        private static List<AssetTask> shader = new List<AssetTask>
        {
            new PrepareTask(),
            new ShaderVariantTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };
        private static List<AssetTask> sprite = new List<AssetTask>
        {
            new PrepareTask(),
            new SpriteAtlasTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> collectAssets = new List<AssetTask>
        {
            new PrepareTask(),
            new RawAssetTask(),
            new CollectAssetsTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> hashPreview = new List<AssetTask>
        {
            new PrepareTask(),
            new RawAssetTask(),
            new CollectAssetsTask(),
            new CollectHashBundleGroupTask(),
            new FastModeManifestTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> clearOutput = new List<AssetTask>
        {
            new PrepareTask(),
            new ClearOutputTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };
        private static List<AssetTask> openOutput = new List<AssetTask>
        {
            new PrepareTask(),
            new OpenOutputTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };
        private static List<AssetTask> clearHistory = new List<AssetTask>
        {
            new PrepareTask(),

            new ClearHistoryTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),

        };

    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;

namespace WooAsset
{
    public class AssetTaskRunner : AssetTask
    {
        protected AssetTaskRunner(List<AssetTask> tasks)
        {
            this.tasks = tasks;
        }
        private List<AssetTask> tasks = new List<AssetTask>();
        private static Queue<AssetTaskRunner> wait = new Queue<AssetTaskRunner>();

        public override float progress => _progress;
        private float _progress = 0;
        private static AssetTaskRunner current;
        protected async override void OnExecute(AssetTaskContext context)
        {
            if (current != null)
            {
                foreach (var item in wait)
                {
                    if (item.context.Pipeline == context.Pipeline)
                    {
                        InvokeComplete();
                        return;
                    }
                }
                wait.Enqueue(this);
            }
            else
            {
                current = this;
                Stopwatch sw = Stopwatch.StartNew();
                await Task.Delay(100);
                for (int i = 0; i < tasks.Count; i++)
                {
                    _progress = (float)i / tasks.Count;
                    await Task.Delay(100);
                    await Execute(tasks[i], context);
                    if (tasks[i].isErr)
                    {
                        AssetsInternal.LogError($"{context.Pipeline} Err");
                        break;
                    }
                }
                if (context.pipelineFinishTasks != null)
                {
                    for (int i = 0; i < context.pipelineFinishTasks.Count; i++)
                    {
                        _progress = (float)i / context.pipelineFinishTasks.Count;
                        await Task.Delay(100);
                        await Execute(context.pipelineFinishTasks[i], context);
                        if (tasks[i].isErr)
                        {
                            AssetsInternal.LogError($"{context.Pipeline} Err");
                            break;
                        }
                    }
                }
                sw.Stop();
                InvokeComplete();
                AssetsInternal.Log($"{context.Pipeline} Finish {sw.Elapsed.ToString(@"G")}");
                current = null;
                if (wait.Count > 0)
                {
                    AssetTaskRunner ex = wait.Dequeue();
                    ex.Execute(ex.context).WarpErr();
                }
            }
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

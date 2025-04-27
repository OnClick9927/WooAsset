using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace WooAsset
{
    public class AssetTaskRunner : AssetTask
    {
        internal AssetTaskRunner(List<AssetTask> tasks)
        {
            this.tasks = tasks;
        }
        private List<AssetTask> tasks = new List<AssetTask>();

        public override float progress => isDone ? 1 : 0;


        protected async override void OnExecute(AssetTaskContext context)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (tasks != null)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    var task = tasks[i];
                    await Execute(task, context);
                    if (task.isErr)
                    {
                        var err = $"{context.Pipeline}\t\t-->{task.GetType().Name} \t\t {task.error}";
                        AssetsHelper.LogError(err);
                        SetErr(err);
                        break;
                    }
                }
            }
            sw.Stop();
            InvokeComplete();
            AssetsEditorTool.Log($"{context.Pipeline} Finish {sw.Elapsed.ToString(@"G")}");
        }


        [MenuItem(TaskPipelineMenu.EditorSimulate)]
        public static AssetTask EditorSimulate() => EditorSimulate(false, FileNameSearchType.FileName);
        public static AssetTask EditorSimulate(bool fuzzySearch, FileNameSearchType fileNameSearchType)
        {
            var Params = new AssetTaskParams(TaskPipelineType.EditorSimulate) { fuzzySearch = fuzzySearch, fileNameSearchType = fileNameSearchType };

            AssetTask task = Execute(stream_common, Params);
            return task;
        }


        [MenuItem(TaskPipelineMenu.PreviewAssets)]
        public static AssetTask PreviewAssets()
        {
            var Params = new AssetTaskParams(TaskPipelineType.PreviewAssets);
            AssetTask task = Execute(collectasset, Params);
            return task;
        }

        [MenuItem(TaskPipelineMenu.PreviewAllAssets)]
        public static AssetTask PreviewAllAssets()
        {
            var Params = new AssetTaskParams(TaskPipelineType.PreviewAllAssets);
            AssetTask task = Execute(collectAllAssets, Params);
            return task;
        }

        [MenuItem(TaskPipelineMenu.Build)]
        public static AssetTask Build()
        {
            var Params = new AssetTaskParams(TaskPipelineType.BuildBundle);
            AssetTask task = Execute(stream_common, Params);
            return task;
        }
        [MenuItem(TaskPipelineMenu.DryBuild)]
        public static AssetTask DryBuild()
        {
            var Params = new AssetTaskParams(TaskPipelineType.DryBuild);
            AssetTask task = Execute(stream_common, Params);
            return task;
        }

        private static List<AssetTask> stream_common = new List<AssetTask>
        {
            new PrepareTask(),
            new BuildBundleTask(),
            new CollectAssetCrossTask(),
            new CopyToBundlesToServerTask(),
            new CopyBundlesToStreamTask(),
            new BuildExportTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> collectAllAssets = new List<AssetTask>
        {
            new PrepareTask(),
            new CollectAssetsTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

        private static List<AssetTask> collectasset = new List<AssetTask>
        {
            new PrepareTask(),
            new CollectEachPkgAssetTask(),
            new CollectAssetCrossTask(),
            new SetCacheTask(),
            new AssetsEditorTool.CallPipelineFinishTask(),
        };

    }
}



using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    public class CopyToBundlesToServerTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            if (context.Pipeline == TaskPipelineType.BuildBundle)
            {
                try
                {
                    if (!string.IsNullOrEmpty(context.serverDirectory))
                    {

                        string target = AssetsEditorTool.CombinePath(context.serverDirectory, context.buildTargetName, context.version);
                        new CopyDirectoryCMD(context.outputPath, target).Execute();
                        string version = AssetsEditorTool.CombinePath(context.serverDirectory, context.buildTargetName, context.VersionCollectionName);
                        if (AssetsEditorTool.ExistsFile(version))
                            AssetsEditorTool.DeleteFile(version);
                        AssetsEditorTool.MoveFile(AssetsEditorTool.CombinePath(target, context.VersionCollectionName), version);

                    }
                }
                catch (System.Exception e)
                {
                    this.SetErr(e.Message);
                }
            }
            InvokeComplete();
        }
    }
}

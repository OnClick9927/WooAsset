﻿

using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    public class CopyToBundlesToServerTask : AssetTask
    {
        protected async override void OnExecute(AssetTaskContext context)
        {
            if (context.Pipeline == TaskPipelineType.BuildBundle)
            {
                try
                {
                    if (!string.IsNullOrEmpty(context.serverDirectory))
                    {

                        string target = AssetsEditorTool.CombinePath(context.serverDirectory, context.buildTargetName, context.version);
                        await new CopyDirectoryOperation(context.outputPath, target, true);
                        string version = AssetsEditorTool.CombinePath(context.serverDirectory, context.buildTargetName, context.VersionCollectionName);
                        if (AssetsHelper.ExistsFile(version))
                            AssetsEditorTool.DeleteFile(version);
                        AssetsEditorTool.MoveFile(AssetsHelper.CombinePath(target, context.VersionCollectionName), version);

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

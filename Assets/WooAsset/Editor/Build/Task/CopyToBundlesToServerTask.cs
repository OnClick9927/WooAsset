namespace WooAsset
{
    public class CopyToBundlesToServerTask : AssetTask
    {
        protected async override void OnExecute(AssetTaskContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(context.serverDirectory))
                {
                    string target = AssetsHelper.CombinePath(context.serverDirectory, context.buildTargetName);
                    await new CopyBundleOperation(context.outputPath, target, true);
                }
            }
            catch (System.Exception e)
            {
                this.SetErr(e.Message);
            }
            InvokeComplete();
        }
    }
}

namespace WooAsset
{
    public class ClearOutputTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            AssetsHelper.DeleteDirectory(context.outputPath);
            InvokeComplete();
        }
    }
}

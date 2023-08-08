namespace WooAsset
{
    public class ClearHistoryTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            AssetsHelper.DeleteDirectory(context.historyPath);
            InvokeComplete();
        }
    }
}

using System.IO;

namespace WooAsset
{
    public class ClearHistoryTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            Directory.Delete(context.historyPath, true);
            InvokeComplete();
        }
    }
}

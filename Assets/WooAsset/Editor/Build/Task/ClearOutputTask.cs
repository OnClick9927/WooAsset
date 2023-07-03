using System.IO;

namespace WooAsset
{
    public class ClearOutputTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            Directory.Delete(context.outputPath, true);
            InvokeComplete();
        }
    }
}

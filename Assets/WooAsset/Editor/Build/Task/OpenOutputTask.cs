using UnityEditor;

namespace WooAsset
{
    public class OpenOutputTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            EditorUtility.OpenWithDefaultApp(context.outputPath);
            InvokeComplete();
        }
    }
}

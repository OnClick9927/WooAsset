using System.Linq;

namespace WooAsset
{
    public class BuildExportTask : AssetTask
    {
        protected async override void OnExecute(AssetTaskContext context)
        {
            if (context.isNormalBuildMode)
            {

                if (context.cleanHistory)
                {
                    var useful = AssetsHelper.GetDirectoryFiles(context.outputPath)
                               .Select(x => AssetsHelper.GetFileName(x))
                              .ToList();
                    AssetsHelper.GetDirectoryFiles(context.historyPath)
                            .Where(x => !useful.Contains(AssetsHelper.GetFileName(x)))
                            .ToList()
                            .ForEach(x => AssetsEditorTool.DeleteFile(x));
                }
            }
            foreach (var item in context.exports)
            {
                await AssetsEditorTool.WriteObject(item,
                     AssetsHelper.CombinePath(context.outputPath, $"Export_{item.pkg.name}.json"),
                     true
                     );
            }
            InvokeComplete();
        }
    }
}

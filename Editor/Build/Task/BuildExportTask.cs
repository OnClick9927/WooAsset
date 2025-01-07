using System.Linq;

namespace WooAsset
{
    public class BuildExportTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            if (context.Pipeline == TaskPipelineType.BuildBundle)
            {

                if (context.cleanHistory)
                {
                    var useful = AssetsEditorTool.GetDirectoryFiles(context.outputPath)
                               .Select(x => AssetsEditorTool.GetFileName(x))
                              .ToList();
                    AssetsEditorTool.GetDirectoryFiles(context.historyPath)
                            .Where(x => !useful.Contains(AssetsEditorTool.GetFileName(x)))
                            .ToList()
                            .ForEach(x => AssetsEditorTool.DeleteFile(x));
                }
            }
            if (context.Pipeline == TaskPipelineType.BuildBundle || context.Pipeline == TaskPipelineType.DryBuild)
            {

                foreach (var item in context.exports)
                {
                    AssetsEditorTool.WriteJson(item,
                         AssetsEditorTool.CombinePath(context.outputPath, $"Export_{item.pkg.name}.json"));
                }

            }
            InvokeComplete();
        }
    }
}

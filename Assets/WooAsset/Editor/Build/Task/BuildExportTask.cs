using System.Collections.Generic;
using System.Linq;
using static WooAsset.FileData;

namespace WooAsset
{
    public class BuildExportTask : AssetTask
    {
        protected async override void OnExecute(AssetTaskContext context)
        {
            List<FileData> old = context.files;
            List<FileData> files = AssetsHelper.GetDirectoryFiles(context.outputPath).ToList().ConvertAll(x => FileData.CreateByFile(x));
            List<FileData> _change; List<FileData> delete; List<FileData> add;
            FileData.Compare(old, files, FileCompareType.Hash, out _change, out delete, out add);
            context.fileChange = new FileChange() { change = _change, delete = delete, add = add };
            if (context.cleanHistory)
            {
                AssetsHelper.GetDirectoryFiles(context.historyPath)
                        .Where(x => !context.useful.Contains(AssetsHelper.GetFileName(x)))
                        .ToList()
                        .ForEach(x => AssetsHelper.DeleteFile(x));
            }
            AssetsHelper.GetDirectoryFiles(context.outputPath)
                      .Where(x => !context.useful.Contains(AssetsHelper.GetFileName(x)))
                      .ToList()
                      .ForEach(x => AssetsHelper.DeleteFile(x));

            await AssetsEditorTool.WriteObject(new BuildBundleExprotData()
            {
                encrypt = context.encrypt.ToString(),
                buildPkgs = context.buildPkgs,
                version = context.version,
                compress = context.compress.ToString(),
                forceRebuild = context.forceRebuild,
                typeTreeOption = context.typeTreeOption,
                fileChange = context.fileChange,
                versions = context.outputVersions,
            },
                 AssetsHelper.CombinePath(context.outputPath, "BundleExprot.json"),
                 true
                 );
            foreach (var item in context.exports)
            {
                await AssetsEditorTool.WriteObject(item,
                     AssetsHelper.CombinePath(context.outputPath, $"Export_{item.buildPkg.name}.json"),
                     true
                     );
            }
            InvokeComplete();
        }
    }
}

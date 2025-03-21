using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            if (context.Pipeline == TaskPipelineType.BuildBundle && context.copyToStream)
            {
                List<string> buildInConfigs = new List<string>
                {
                    AssetsEditorTool.VersionDataName,
                    AssetsEditorTool.VersionCollectionName,
                }
                .Concat(context.buildPkgs.Select(x => x.manifestFileName))
                .ToList();
                var cmd = new CopyToStreamCMD(context.outputPath, context.version, context.streamBundleDirectory);
                var files = context.buildInBundleSelector.Select(cmd.files, context.buildInAssets,
                    buildInConfigs, context.mergedManifest, context.exports);
                cmd.Execute(files);

                AssetDatabase.Refresh();


            }

            InvokeComplete();
        }
    }
}

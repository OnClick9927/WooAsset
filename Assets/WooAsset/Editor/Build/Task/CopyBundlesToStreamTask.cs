using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        class CopyToStreamCMD : CopyDirectoryCMD
        {
            private string version;

            public CopyToStreamCMD(string srcPath, string version, string targetPath) : base(srcPath, targetPath)
            {
                this.version = version;
            }

            public void Execute(string[] files)
            {
                this.files = files;
                Execute();
            }
            public override void Execute()
            {
                var list = this.files.Select(x => GetTargetFileName(x)).ToList();
                AssetsEditorTool.WriteBufferObjectSync(new StreamBundlesData()
                {
                    fileNames = list.ToArray(),
                    version = this.version,
                },
                AssetsEditorTool.CombinePath(targetPath, StreamBundlesData.fileName));
                base.Execute();
            }

            protected override string GetTargetFileName(string src)
            {
                return $"{base.GetTargetFileName(src)}{StreamBundlesData.fileExt}";
            }
        }

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

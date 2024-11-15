using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        internal class CopyToStreamCMD : CopyDirectoryCMD
        {
            private List<string> buildInConfigs;

            public CopyToStreamCMD(string srcPath, string targetPath) : base(srcPath, targetPath)
            {
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
                },
                AssetsEditorTool.CombinePath(targetPath, StreamBundlesData.fileName));
                base.Execute();
            }

            protected override string GetTargetFileName(string src)
            {
                return $"{base.GetTargetFileName(src)}{StreamBundlesData.fileExt}";
            }
        }
    }
}

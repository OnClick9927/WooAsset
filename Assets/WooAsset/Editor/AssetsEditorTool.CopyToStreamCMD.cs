﻿using System.Linq;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        internal class CopyToStreamCMD : CopyDirectoryCMD
        {
            private string version;

            public CopyToStreamCMD(string srcPath,string version, string targetPath) : base(srcPath, targetPath)
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
    }
}

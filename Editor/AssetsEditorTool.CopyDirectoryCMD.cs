namespace WooAsset
{
    partial class AssetsEditorTool
    {

        internal class CopyDirectoryCMD
        {
            protected readonly string targetPath;
            public string[] files;
            public CopyDirectoryCMD(string srcPath, string targetPath)
            {
                this.targetPath = targetPath;
                if (!AssetsEditorTool.ExistsDirectory(srcPath))
                {
                    AssetsEditorTool.LogError("Path not Exist");
                }
                else
                {
                    files = AssetsEditorTool.GetDirectoryFiles(srcPath);
                    if (AssetsEditorTool.ExistsDirectory(targetPath))
                        AssetsEditorTool.DeleteDirectory(targetPath);
                    AssetsEditorTool.CreateDirectory(targetPath);
                }
            }
            protected virtual string GetTargetFileName(string src) => AssetsEditorTool.GetFileName(src);
            public virtual void Execute()
            {
                if (files == null) return;
                foreach (var path in files)
                {
                    string _destPath = AssetsEditorTool.CombinePath(targetPath, GetTargetFileName(path));
                    AssetsEditorTool.CopyFile(path, _destPath);
                }
            }
        }
    }
}

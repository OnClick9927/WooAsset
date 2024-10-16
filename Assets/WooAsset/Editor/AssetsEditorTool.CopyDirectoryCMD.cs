namespace WooAsset
{
    partial class AssetsEditorTool
    {

        internal class CopyDirectoryCMD
        {
            private readonly string srcPath;
            protected readonly string targetPath;
            private bool _cover;
            protected string[] files;
            private int step = 0;
            public CopyDirectoryCMD(string srcPath, string targetPath)
            {
                this.srcPath = srcPath;
                this.targetPath = targetPath;
                Copy();
            }
            protected virtual void Copy()
            {

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
                    Done();
                }
            }
            protected virtual string GetTargetFileName(string src) => AssetsEditorTool.GetFileName(src);
            protected virtual bool NeedCopy(string src) { return true; }
            protected virtual void Done()
            {
                foreach (var path in files)
                {
                    if (!NeedCopy(path)) continue;
                    string _destPath = AssetsEditorTool.CombinePath(targetPath, GetTargetFileName(path));
                    AssetsEditorTool.CopyFile(path, _destPath);
                    step++;
                }
            }
        }
    }
}

namespace WooAsset
{
    public class CopyBundleOperation : AssetOperation
    {
        private readonly string srcPath;
        private readonly string destPath;
        private bool _cover;
        private string[] files;
        private int step = 0;
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return step / (float)files.Length;
            }
        }
        public CopyBundleOperation(string srcPath, string destPath, bool cover)
        {
            _cover = cover;
            this.srcPath = srcPath;
            this.destPath = destPath;
            Copy();
        }
        protected virtual void Copy()
        {
            if (!AssetsHelper.ExistsDirectory(srcPath))
            {
                this.SetErr($"source directory not exist {srcPath}");
                this.InvokeComplete();
            }
            else
            {
                files = AssetsHelper.GetDirectoryFiles(srcPath);
                Done();
            }
        }
        protected virtual string GetDestFileName(string src) => AssetsHelper.GetFileName(src);
        protected virtual bool NeedCopy(string src) { return true; }
        private async void Done()
        {
            AssetsHelper.CreateDirectory(destPath);

            foreach (var path in files)
            {
                if (!NeedCopy(path)) continue;
                string _destPath = AssetsHelper.CombinePath(destPath, GetDestFileName(path));
                if (AssetsHelper.ExistsFile(_destPath) && !_cover) continue;
                await AssetsHelper.CopyFromFile(path, _destPath);
                step++;
            }
            this.InvokeComplete();
        }
    }


}

﻿namespace WooAsset
{
    public class CopyDirectoryOperation : Operation
    {
        private readonly string srcPath;
        protected readonly string destPath;
        private bool _cover;
        protected string[] files;
        private int step = 0;
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return step / (float)files.Length;
            }
        }
        public CopyDirectoryOperation(string srcPath, string destPath, bool cover)
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
                this.InvokeComplete();
            }
            else
            {
                files = AssetsHelper.GetDirectoryFiles(srcPath);
                AssetsHelper.CreateDirectory(destPath);
                Done();
            }
        }
        protected virtual string GetDestFileName(string src) => AssetsHelper.GetFileName(src);
        protected virtual bool NeedCopy(string src) { return true; }
        protected virtual async void Done()
        {

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

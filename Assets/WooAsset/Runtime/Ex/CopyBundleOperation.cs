

using System.Collections.Generic;
using System.IO;

namespace WooAsset
{


    public class CopyBundleOperation : AssetOperation
    {
        private readonly string srcPath;
        private readonly string destPath;
        private bool _cover;
        private FileInfo[] files;
        private DirectoryInfo[] dirs;
        private int step = 0;
        private Queue<CopyBundleOperation> queue = new Queue<CopyBundleOperation>();
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                float sum = step / files.Length;
                if (queue.Count > 0)
                    sum += queue.Peek().progress;
                sum += (dirs.Length - queue.Count);
                return sum / (dirs.Length + 1);
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
            if (!Directory.Exists(srcPath))
            {
                this.SetErr($"source directory not exist {srcPath}");
                this.InvokeComplete();
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                files = dir.GetFiles();
                dirs = dir.GetDirectories();
                Done();
            }
        }
        protected virtual string GetDestFileName(FileInfo src) => src.Name;
        protected virtual bool NeedCopy(FileInfo src) { return true; }
        private async void Done()
        {
            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);
            foreach (var item in dirs)
            {
                string _destPath = AssetsInternal.CombinePath(destPath, item.Name);
                queue.Enqueue(new CopyBundleOperation(item.FullName, _destPath, _cover));
                while (queue.Count > 0)
                {
                    await queue.Peek();
                    queue.Dequeue().WarpErr();
                }
            }
            foreach (var item in files)
            {
                if (!NeedCopy(item)) continue;
                string _destPath = AssetsInternal.CombinePath(destPath, GetDestFileName(item));
                if (File.Exists(_destPath) && !_cover) continue;
                using (FileStream SourceStream = File.Open(item.FullName, FileMode.Open))
                {
                    using (FileStream DestinationStream = File.Create(_destPath))
                    {
                        await SourceStream.CopyToAsync(DestinationStream);
                    }
                }
                step++;

            }
            this.InvokeComplete();
        }
    }


}

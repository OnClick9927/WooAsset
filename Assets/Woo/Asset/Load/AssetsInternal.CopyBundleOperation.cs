

using System.Collections.Generic;
using System.IO;

namespace WooAsset
{
    partial class AssetsInternal
    {
        public class CopyBundleOperation : AssetOperation
        {
            private readonly string srcPath;
            private readonly string destPath;
            private FileInfo[] fileinfos;
            private DirectoryInfo[] dirinfos;
            private int step = 0;
            private Queue<CopyBundleOperation> queue = new Queue<CopyBundleOperation>();
            public override float progress
            {
                get
                {
                    if (isDone) return 1;
                    float sum = step / fileinfos.Length;
                    if (queue.Count > 0)
                        sum += queue.Peek().progress;
                    sum += (dirinfos.Length - queue.Count);
                    return sum / (dirinfos.Length + 1);
                }
            }
            public CopyBundleOperation(string srcPath, string destPath)
            {
                this.srcPath = srcPath;
                this.destPath = destPath;
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                fileinfos = dir.GetFiles();
                dirinfos = dir.GetDirectories();
                Done();

            }
            private async void Done()
            {
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }
                foreach (var item in dirinfos)
                {
                    string _destpath = CombinePath(destPath,item.Name);
                    queue.Enqueue(new CopyBundleOperation(item.FullName, _destpath));
                    while (queue.Count > 0)
                    {
                        await queue.Peek();
                        queue.Dequeue();
                    }
                }
                foreach (var item in fileinfos)
                {
                    string _destpath = CombinePath(destPath, item.Name);

                    using (FileStream SourceStream = File.Open(item.FullName, FileMode.Open))
                    {
                        using (FileStream DestinationStream = File.Create(_destpath))
                        {
                            await SourceStream.CopyToAsync(DestinationStream);
                            step++;
                        }
                    }
                }
                this.InvokeComplete();
            }
        }
    }
  
}

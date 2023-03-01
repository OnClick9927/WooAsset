

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
            public CopyBundleOperation(string srcPath, string destPath)
            {
                if (!Directory.Exists(srcPath))
                {
                    this.SetErr($"source directory not exist {srcPath}");
                    this.InvokeComplete();

                }
                else
                {
                    this.srcPath = srcPath;
                    this.destPath = destPath;
                    DirectoryInfo dir = new DirectoryInfo(srcPath);
                    files = dir.GetFiles();
                    dirs = dir.GetDirectories();
                    Done();
                }


            }
            private async void Done()
            {
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);
                foreach (var item in dirs)
                {
                    string _destPath = CombinePath(destPath, item.Name);
                    queue.Enqueue(new CopyBundleOperation(item.FullName, _destPath));
                    while (queue.Count > 0)
                    {
                        await queue.Peek();
                        queue.Dequeue();
                    }
                }
                foreach (var item in files)
                {
                    string _destPath = CombinePath(destPath, item.Name);
                    if (File.Exists(_destPath))
                    {
                        continue;
                    }
                    else
                    {
                        using (FileStream SourceStream = File.Open(item.FullName, FileMode.Open))
                        {
                            using (FileStream DestinationStream = File.Create(_destPath))
                            {
                                await SourceStream.CopyToAsync(DestinationStream);
                            }
                        }
                    }
                    step++;

                }
                this.InvokeComplete();
            }
        }
    }

}

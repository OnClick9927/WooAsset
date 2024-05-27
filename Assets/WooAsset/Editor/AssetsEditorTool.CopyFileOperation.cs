using System;
using System.Threading.Tasks;
using System.IO;

namespace WooAsset
{
    public partial class AssetsEditorTool
    {
        private class CopyFileOperation : Operation
        {
            private int n;
            private string targetPath;
            public override float progress => isDone ? 1 : _progress;
            private float _progress;
            public CopyFileOperation(string targetPath, string srcPath, int n = 1024 * 1024)
            {
                this.n = n;
                this.targetPath = targetPath;
                CopyFromFile(srcPath);
            }

            private async void CopyFromFile(string srcPath)
            {
                byte[] buffer = new byte[n];
                int offset = 0;
                using (FileStream fs = File.OpenRead(srcPath))
                {
                    long len = fs.Length;
                    long last = len;
                    using (FileStream _fs = File.OpenWrite(targetPath))
                    {
                        _fs.Seek(0, SeekOrigin.Begin);
                        while (last > 0)
                        {
                            var read = fs.Read(buffer, 0, (int)Math.Min(n, last));
                            _fs.Write(buffer, 0, read);
                            offset += read;
                            last -= read;
                            _progress = offset / (float)len;
                            if (last <= 0) break;

                            await Task.Delay(1);
                        }
                    }
                }
                InvokeComplete();
            }

        }

    }
}

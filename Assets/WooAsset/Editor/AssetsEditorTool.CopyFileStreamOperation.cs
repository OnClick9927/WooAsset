using System;
using System.IO;

namespace WooAsset
{
    partial class AssetsEditorTool
    {

        class CopyFileStreamOperation : Operation
        {
            private int n;
            public CopyFileStreamOperation(string srcPath, Stream target, int n = 1024 * 1024)
            {
                this.n = n;
                WriteToStream(srcPath, target);
            }

            public override float progress => isDone ? 1 : _progress;
            private float _progress;
            private async void WriteToStream(string srcPath, Stream target)
            {
                byte[] buffer = new byte[n];
                int offset = 0;
                using (FileStream fs = File.OpenRead(srcPath))
                {
                    long len = fs.Length;
                    long last = len;
                    while (last > 0)
                    {
                        var read = fs.Read(buffer, 0, (int)Math.Min(n, last));
                        target.Write(buffer, 0, read);
                        offset += read;
                        last -= read;
                        _progress = offset / (float)len;
                        if (last <= 0) break;

                        await new YieldOperation();
                    }
                }
                InvokeComplete();
            }

        }
    }

}

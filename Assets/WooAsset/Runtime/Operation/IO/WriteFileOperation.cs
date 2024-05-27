using System;
using System.IO;

namespace WooAsset
{

    class WriteFileOperation : Operation
    {
        private int n;
        private string targetPath;
        public override float progress => isDone ? 1 : _progress;
        private float _progress;
        public WriteFileOperation(string targetPath, byte[] bytes, bool async, int n = 1024 * 1024)
        {
            this.n = n;
            this.targetPath = targetPath;
            CopyFromBytes(bytes, async);
        }
        private async void CopyFromBytes(byte[] bytes, bool async)
        {
            if (async)
            {
                int offset = 0;
                long len = bytes.Length;
                long last = len;
                using (FileStream _fs = File.OpenWrite(targetPath))
                {
                    _fs.Seek(0, SeekOrigin.Begin);

                    while (last > 0)
                    {
                        var read = (int)Math.Min(n, last);
                        _fs.Write(bytes, offset, read);
                        offset += read;
                        last -= read;
                        _progress = offset / (float)len;
                        if (last <= 0) break;

                        await new YieldOperation();
                    }
                }
            }
            else
            {
                File.WriteAllBytes(targetPath, bytes);
            }

            InvokeComplete();
        }

    }
}

using System;
using System.IO;

namespace WooAsset
{
     class CopyFileOperation : Operation
    {
        private int n;
        private string targetPath;
        public override float progress => isDone ? 1 : _progress;
        private float _progress;
        public CopyFileOperation(string targetPath, int n = 1024 * 1024)
        {
            this.n = n;
            this.targetPath = targetPath;
        }
        public async void CopyFromFile(string srcPath)
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

                        await new YieldOperation();
                    }
                }
            }
            InvokeComplete();
        }
        public async void CopyFromBytes(byte[] bytes, bool async)
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

        public async void WriteToStream(string srcPath, Stream target)
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

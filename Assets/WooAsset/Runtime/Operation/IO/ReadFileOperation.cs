using System;
using System.IO;

namespace WooAsset
{
    public class ReadFileOperation : Operation
    {
        private int n;
        private string path;
        public byte[] bytes;
        private bool async;

        public override float progress => _progress;
        private float _progress;
        public ReadFileOperation(string path, bool async, int n = 8192)
        {
            this.n = n;
            this.path = path;
            this.async = async;
            Done();
        }
        private async void Done()
        {
            if (async)
            {
                int offset = 0;
                using (FileStream fs = File.OpenRead(path))
                {
                    long len = fs.Length;
                    bytes = new byte[len];
                    long last = len;
                    while (last > 0)
                    {
                        var read = fs.Read(bytes, offset, (int)Math.Min(n, last));
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
                bytes = File.ReadAllBytes(path);
            }
            InvokeComplete();
        }
    }
}

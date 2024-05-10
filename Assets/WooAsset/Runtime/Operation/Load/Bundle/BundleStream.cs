using System.Collections.Generic;
using System.IO;

namespace WooAsset
{
    public class BundleStream : FileStream
    {
        private static Queue<BundleStream> streams = new Queue<BundleStream>();
        public static void CloseStreams()
        {
#if UNITY_EDITOR
            AssetsHelper.Log($"clear file {streams.Count} streams for editor");
            while (streams.Count > 0)
            {
                streams
                    .Dequeue().Dispose();

            }
#endif
        }
        private readonly string bundleName;
        private IAssetStreamEncrypt encrypt;
        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share, string bundleName, IAssetStreamEncrypt encrypt) : base(path, mode, access, share)
        {
            this.bundleName = bundleName;
            this.encrypt = encrypt;
            streams.Enqueue(this);
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            EncryptBuffer.Decode(bundleName, array, offset, count, encrypt);

            return index;
        }
    }

}

using System.Collections.Generic;
using System.IO;

namespace WooAsset
{
    public class BundleStream : FileStream
    {
        private static List<BundleStream> streams = new List<BundleStream>();
        public static void CloseStreams()
        {
#if UNITY_EDITOR
            if (streams.Count > 0)
            {
                AssetsHelper.Log($"clear file {streams.Count} streams for editor");
                for (int i = 0; i < streams.Count; i++)
                    streams[i].Dispose();
            }
#endif
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                streams.Remove(this);
            }
        }
        private readonly string bundleName;
        private IAssetEncrypt encrypt;
        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share, string bundleName, IAssetEncrypt encrypt) : base(path, mode, access, share)
        {
            this.bundleName = bundleName;
            this.encrypt = encrypt;
#if UNITY_EDITOR
            streams.Add(this);
#endif
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            EncryptBuffer.Decode(bundleName, array, offset, count, encrypt);

            return index;
        }
    }

}

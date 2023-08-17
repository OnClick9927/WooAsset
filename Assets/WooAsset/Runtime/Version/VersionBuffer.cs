using System.Text;
using UnityEngine;

namespace WooAsset
{
    public class VersionBuffer
    {
        private static string _localHashName, _remoteHashName;
        public static string localHashName
        {
            get
            {
                if (string.IsNullOrEmpty(_localHashName))
                {
                    _localHashName = AssetsHelper.GetStringHash("#########version###");
                }
                return _localHashName;
            }
        }
        public static string remoteHashName
        {
            get
            {
                if (string.IsNullOrEmpty(_remoteHashName))
                {
                    _remoteHashName = AssetsHelper.GetStringHash("#########version");
                }
                return _remoteHashName;
            }
        }
        private static Encoding encoding = Encoding.Default;
        public class WriteBufferOperation<T> : Operation
        {
            private CopyFileOperation op;
            public override float progress => isDone ? 1 : (op == null ? 0 : op.progress);
            private T version;
            private string path;
            private IAssetStreamEncrypt en;
            private bool go;
            public WriteBufferOperation(T version, string path, IAssetStreamEncrypt en, bool go)
            {
                this.version = version;
                this.path = path;
                this.en = en;
                this.go = go;
                Done();

            }
            private async void Done()
            {
                if (go)
                {
                    var bytes = encoding.GetBytes(JsonUtility.ToJson(version,true));
                    var buffer = EncryptBuffer.Encode(remoteHashName, bytes, en);
                    op = AssetsHelper.WriteFile(buffer, path, true);
                    await op;
                }
                InvokeComplete();
            }
        }

        private static T Read<T>(byte[] bytes, IAssetStreamEncrypt en) => JsonUtility.FromJson<T>(encoding.GetString(EncryptBuffer.Decode(remoteHashName, bytes, en)));
        private static WriteBufferOperation<T> Write<T>(T version, string path, IAssetStreamEncrypt en, bool go) => new WriteBufferOperation<T>(version, path, en, go);
        public static AssetsVersionCollection.VersionData ReadVersionData(byte[] bytes, IAssetStreamEncrypt en)
        {
            return Read<AssetsVersionCollection.VersionData>(bytes, en);
        }
        public static WriteBufferOperation<AssetsVersionCollection.VersionData> WriteVersionData(AssetsVersionCollection.VersionData version, string path, IAssetStreamEncrypt en) => Write(version, path, en, !(Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()));
        public static BundlesVersion ReadBundleVersion(byte[] bytes, IAssetStreamEncrypt en) => Read<BundlesVersion>(bytes, en);
        public static WriteBufferOperation<BundlesVersion> WriteBundlesVersion(BundlesVersion version, string path, IAssetStreamEncrypt en) => Write(version, path, en, !(Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()));
        public static ManifestData ReadManifest(byte[] bytes, IAssetStreamEncrypt en) => Read<ManifestData>(bytes, en);
        public static WriteBufferOperation<ManifestData> WriteManifest(ManifestData version, string path, IAssetStreamEncrypt en) => Write(version, path, en, !(Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()));


        public static AssetsVersionCollection ReadAssetsVersionCollection(byte[] bytes, IAssetStreamEncrypt en) => Read<AssetsVersionCollection>(bytes, en);
        public static WriteBufferOperation<AssetsVersionCollection> WriteAssetsVersionCollection(AssetsVersionCollection version, string path, IAssetStreamEncrypt en) => Write(version, path, en, !(Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()));
    }
}

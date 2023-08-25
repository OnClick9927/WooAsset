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

        private static T Read<T>(byte[] bytes, IAssetStreamEncrypt en) => JsonUtility.FromJson<T>(AssetsHelper.encoding.GetString(EncryptBuffer.Decode(remoteHashName, bytes, en)));
        private static WriteVersionBufferOperation<T> Write<T>(T version, string path, IAssetStreamEncrypt en) => new WriteVersionBufferOperation<T>(version, path, en, !(Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()));
        public static AssetsVersionCollection.VersionData ReadVersionData(byte[] bytes, IAssetStreamEncrypt en)
        {
            return Read<AssetsVersionCollection.VersionData>(bytes, en);
        }
        public static Operation WriteVersionData(AssetsVersionCollection.VersionData version, string path, IAssetStreamEncrypt en) => Write(version, path, en);
        public static BundlesVersion ReadBundleVersion(byte[] bytes, IAssetStreamEncrypt en) => Read<BundlesVersion>(bytes, en);
        public static Operation WriteBundlesVersion(BundlesVersion version, string path, IAssetStreamEncrypt en) => Write(version, path, en);
        public static ManifestData ReadManifest(byte[] bytes, IAssetStreamEncrypt en) => Read<ManifestData>(bytes, en);
        public static Operation WriteManifest(ManifestData version, string path, IAssetStreamEncrypt en) => Write(version, path, en);


        public static AssetsVersionCollection ReadAssetsVersionCollection(byte[] bytes, IAssetStreamEncrypt en) => Read<AssetsVersionCollection>(bytes, en);
        public static Operation WriteAssetsVersionCollection(AssetsVersionCollection version, string path, IAssetStreamEncrypt en) => Write(version, path, en);
    }
}

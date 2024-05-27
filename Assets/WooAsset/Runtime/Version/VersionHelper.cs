using UnityEngine;

namespace WooAsset
{

    public class VersionHelper
    {
        public const string localHashName = "localversion.ver";
        public const string remoteHashName = "remoteversion.ver";
        public static string GetManifestFileName(string pkgName) => $"manifest_{pkgName}.ver";
        public static string GetBundleFileName(string pkgName) => $"bundle_{pkgName}.ver";

        private static T Read<T>(byte[] bytes, IAssetStreamEncrypt en) where T:IBufferObject,new() => AssetsHelper.ReadFromBytes<T>(EncryptBuffer.Decode(remoteHashName, bytes, en));


        private static Operation Write<T>(T version, string path, IAssetStreamEncrypt en) where T : IBufferObject
        {
            var go = !(Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying());
            if (!go) return Operation.empty;
            var bytes = AssetsHelper.ObjectToBytes(version);
            return AssetsHelper.WriteFile(EncryptBuffer.Encode(remoteHashName, bytes, en), path, false);
        }

        public static AssetsVersionCollection.VersionData ReadVersionData(byte[] bytes, IAssetStreamEncrypt en)
        {
            return Read<AssetsVersionCollection.VersionData>(bytes, en);
        }
        public static BundlesVersion ReadBundleVersion(byte[] bytes, IAssetStreamEncrypt en) => Read<BundlesVersion>(bytes, en);
        public static ManifestData ReadManifest(byte[] bytes, IAssetStreamEncrypt en) => Read<ManifestData>(bytes, en);
        public static AssetsVersionCollection ReadAssetsVersionCollection(byte[] bytes, IAssetStreamEncrypt en) => Read<AssetsVersionCollection>(bytes, en);
      
        
        public static Operation WriteVersionData(AssetsVersionCollection.VersionData version, string path, IAssetStreamEncrypt en) => Write(version, path, en);
        public static Operation WriteManifest(ManifestData version, string path, IAssetStreamEncrypt en) => Write(version, path, en);
        public static Operation WriteBundlesVersion(BundlesVersion version, string path, IAssetStreamEncrypt en) => Write(version, path, en);
        public static Operation WriteAssetsVersionCollection(AssetsVersionCollection version, string path, IAssetStreamEncrypt en) => Write(version, path, en);
    }
}

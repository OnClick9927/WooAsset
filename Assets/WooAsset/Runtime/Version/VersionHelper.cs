using System.IO;
using UnityEngine;

namespace WooAsset
{

    public class VersionHelper
    {
        public const string VersionDataName = "localversion.ver";
        public const string VersionCollectionName = "remoteversion.ver";
        public static string GetManifestFileName(string pkgName) => $"manifest_{pkgName}.ver";
        //public static string GetBundleFileName(string pkgName) => $"bundle_{pkgName}.ver";

        private static T Read<T>(byte[] bytes) where T : IBufferObject, new() => AssetsHelper.ReadFromBytes<T>(bytes);


        private static Operation Write<T>(T version, string path) where T : IBufferObject
        {
            var go = !(Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying());
            if (!go) return Operation.empty;
            var bytes = AssetsHelper.ObjectToBytes(version);
            return AssetsHelper.WriteFile(bytes, path, false);
        }

        public static VersionData ReadVersionData(byte[] bytes) => Read<VersionData>(bytes);
        //public static BundlesVersionData ReadBundleVersion(byte[] bytes) => Read<BundlesVersionData>(bytes);
        public static ManifestData ReadManifest(byte[] bytes) => Read<ManifestData>(bytes);
        public static VersionCollectionData ReadAssetsVersionCollection(byte[] bytes) => Read<VersionCollectionData>(bytes);


        public static Operation WriteVersionData(VersionData version, string path) => Write(version, path);
        public static Operation WriteManifest(ManifestData version, string path) => Write(version, path);
        //public static Operation WriteBundlesVersion(BundlesVersionData version, string path) => Write(version, path);
        public static Operation WriteAssetsVersionCollection(VersionCollectionData version, string path) => Write(version, path);
    }
}

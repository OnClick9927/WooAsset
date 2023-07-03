using System.IO;
using System.Text;
using UnityEngine;

namespace WooAsset
{
    public class VersionBuffer
    {
        public static string localHashName { get { return AssetsInternal.GetStringHash("#########version###"); } }
        public static string remoteHashName { get { return AssetsInternal.GetStringHash("#########version"); } }
        private static Encoding encoding = Encoding.Default;

        private static T Read<T>(byte[] bytes, IAssetStreamEncrypt en)
        {
            var buffer = EncryptBuffer.Decode(remoteHashName, bytes, en);
            var str = encoding.GetString(buffer);
            return JsonUtility.FromJson<T>(str);
        }
        private static void Write<T>(T version, string path,IAssetStreamEncrypt en)
        {
            var bytes = encoding.GetBytes(JsonUtility.ToJson(version));
            var buffer = EncryptBuffer.Encode(remoteHashName, bytes, en);
            File.WriteAllBytes(path, buffer);
        }
        public static AssetsVersionCollection.VersionData ReadVersionData(byte[] bytes, IAssetStreamEncrypt en)
        {
            return Read<AssetsVersionCollection.VersionData>(bytes,en);
        }
        public static void WriteVersionData(AssetsVersionCollection.VersionData version, string path, IAssetStreamEncrypt en)
        {
            if (Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()) return;
            Write(version, path, en);
        }

        public static BundlesVersion ReadBundleVersion(byte[] bytes, IAssetStreamEncrypt en)
        {
            return Read<BundlesVersion>(bytes, en);
        }
        public static void WriteBundlesVersion(BundlesVersion version, string path, IAssetStreamEncrypt en)
        {
            if (Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()) return;
            Write(version, path, en);
        }
        public static ManifestData ReadManifest(byte[] bytes, IAssetStreamEncrypt en)
        {
            return Read<ManifestData>(bytes, en);
        }
        public static void WriteManifest(ManifestData version, string path, IAssetStreamEncrypt en)
        {
            if (Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()) return;
            Write(version, path, en);
        }


        public static AssetsVersionCollection ReadAssetsVersionCollection(byte[] bytes, IAssetStreamEncrypt en)
        {
            return Read<AssetsVersionCollection>(bytes, en);

        }
        public static void WriteAssetsVersionCollection(AssetsVersionCollection version, string path, IAssetStreamEncrypt en)
        {
            if (Application.isPlaying && !AssetsInternal.GetSaveBundlesWhenPlaying()) return;
            Write(version, path, en);

        }
    }
}

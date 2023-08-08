using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace WooAsset
{
    public static class AssetsHelper
    {
        public class VersionBuffer
        {
            public static string localHashName { get { return AssetsHelper.GetStringHash("#########version###"); } }
            public static string remoteHashName { get { return AssetsHelper.GetStringHash("#########version"); } }
            private static Encoding encoding = Encoding.Default;
            public class WriteBufferOperation<T> : AssetOperation
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
                        var bytes = encoding.GetBytes(JsonUtility.ToJson(version));
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

        public class ReadFileOperation : AssetOperation
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
        public class CopyFileOperation : AssetOperation
        {
            private int n;
            private string targetPath;
            public override float progress => isDone ? 1 : _progress;
            private float _progress;
            public CopyFileOperation(string targetPath, int n = 8192)
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
                        while (last > 0)
                        {
                            var read = fs.Read(buffer, 0, (int)Math.Min(n, last));
                            _fs.Write(buffer, 0, read);
                            offset += read;
                            last -= read;
                            _progress = offset / (float)len;
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
                        while (last > 0)
                        {
                            var read = (int)Math.Min(n, last);
                            _fs.Write(bytes, offset, read);
                            offset += read;
                            last -= read;
                            _progress = offset / (float)len;
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
                        await new YieldOperation();
                    }
                }
                InvokeComplete();
            }

        }
        public static CopyFileOperation CopyFromFile(string srcPath, string targetPath)
        {
            CopyFileOperation c = new CopyFileOperation(targetPath);
            c.CopyFromFile(srcPath);
            return c;
        }
        public static CopyFileOperation WriteFile(byte[] bytes, string targetPath, bool async)
        {
            CopyFileOperation c = new CopyFileOperation(targetPath);
            c.CopyFromBytes(bytes, async);
            return c;
        }
        public static CopyFileOperation WriteStream(string srcPath, Stream target)
        {
            CopyFileOperation c = new CopyFileOperation(srcPath);
            c.WriteToStream(srcPath, target);
            return c;
        }

        public static ReadFileOperation ReadFile(string srcPath, bool async) => new ReadFileOperation(srcPath, async);
        public static bool ExistsFile(string path) => File.Exists(path);
        public static bool ExistsDirectory(string path) => Directory.Exists(path);
        public static void DeleteDirectory(string path) => Directory.Delete(path, true);
        public static void DeleteFile(string path) => File.Delete(path);
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public static bool IsDirectory(string path) => Directory.Exists(path);
        public static string[] GetDirectoryFiles(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        public static string[] GetDirectoryDirectories(string path) => Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

        private static string ToHashString(byte[] bytes)
        {
            byte[] retVal = MD5.Create().ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetStringHash(string str) => ToHashString(Encoding.Default.GetBytes(str));

        public static string GetFileHash(string path) => ExistsFile(path) ? ToHashString(File.ReadAllBytes(path)) : string.Empty;
        public static long GetFileLength(string path) => ExistsFile(path) ? new FileInfo(path).Length : 0;
        public static string GetFileName(string path) => Path.GetFileName(path);
        public static string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);
        public static string GetFileExtension(string path) => Path.GetExtension(path);


        public static string GetDirectoryName(string path) => Path.GetDirectoryName(path);


        public static string CombinePath(string self, string combine) => Path.Combine(self, combine);
        public static string ToRegularPath(string path) => path.Replace('\\', '/');
        public static string ToAssetsPath(string self) => "Assets" + Path.GetFullPath(self).Substring(Path.GetFullPath(Application.dataPath).Length).Replace("\\", "/");

        public static void LogWarning(string msg) => Debug.LogWarning("Assets : " + msg);
        public static void Log(string msg) => Debug.Log("Assets : " + msg);
        public static void LogError(string err) => Debug.LogError("Assets : " + err);
    }
}

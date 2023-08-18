using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace WooAsset
{

    public static class AssetsHelper
    {


        public static T ReadObject<T>(byte[] bytes) => JsonUtility.FromJson<T>(AssetsHelper.encoding.GetString(bytes));
        public static Operation WriteObject<T>(T t, string path, bool async) => new WriteObjectOperation<T>(t, path, async);
        internal static Encoding encoding = Encoding.Default;


        public static Operation CopyFromFile(string srcPath, string targetPath)
        {
            CopyFileOperation c = new CopyFileOperation(targetPath);
            c.CopyFromFile(srcPath);
            return c;
        }
        public static Operation WriteFile(byte[] bytes, string targetPath, bool async)
        {
            CopyFileOperation c = new CopyFileOperation(targetPath);
            c.CopyFromBytes(bytes, async);
            return c;
        }
        public static Operation WriteStream(string srcPath, Stream target)
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
        public static string[] GetDirectoryFiles(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(x => AssetsHelper.ToRegularPath(x)).ToArray();
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
        public static string GetStringHash(string str) => ToHashString(encoding.GetBytes(str));

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

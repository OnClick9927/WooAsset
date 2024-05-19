using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace WooAsset
{

    public static class AssetsHelper
    {
        public static string buildTarget
        {
            get
            {
#if UNITY_EDITOR
                switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
                {
                    case UnityEditor.BuildTarget.Android:
                        return "Android";
                    case UnityEditor.BuildTarget.StandaloneWindows:
                    case UnityEditor.BuildTarget.StandaloneWindows64:
                        return "Windows";
                    case UnityEditor.BuildTarget.iOS:
                        return "iOS";
                    case UnityEditor.BuildTarget.WebGL:
                        return "WebGL";
                    case UnityEditor.BuildTarget.StandaloneOSX:
                        return "OSX";
                }
#else
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer: return "Windows";
                    case RuntimePlatform.Android: return "Android";
                    case RuntimePlatform.IPhonePlayer: return "iOS";
                    case RuntimePlatform.WebGLPlayer: return "WebGL";
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.OSXEditor: return "OSX";

                }
#endif
                return string.Empty;
            }
        }
        public static string streamBundleDirectory => CombinePath(Application.streamingAssetsPath, buildTarget);

        public static T ReadObject<T>(byte[] bytes) => JsonUtility.FromJson<T>(encoding.GetString(bytes));
        public static Encoding encoding = Encoding.Default;

        public static Value GetFromDictionary<Key, Value>(Dictionary<Key, Value> map, Key key) where Value : class, new()
        {
            Value t;
            if (!map.TryGetValue(key, out t))
            {
                t = new Value();
                map.Add(key, t);
            }
            return t;
        }
        public static Value GetOrDefaultFromDictionary<Key, Value>(Dictionary<Key, Value> map, Key key) where Value : class
        {
            Value t;
            if (map.TryGetValue(key, out t))
                return t;
            return null;
        }



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
        public static string[] GetDirectoryFiles(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(x => ToRegularPath(x)).ToArray();
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace WooAsset
{
    public class AssetsHelper
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
        public static string StreamBundlePath => $"{Application.streamingAssetsPath}/{buildTarget}";


        public static bool log_Enable = true;
        public static bool warn_Enable = true;
        public static bool err_Enable = true;

        public static void LogWarning(string msg)
        {
            if (!warn_Enable) return;
            Debug.LogWarning("Assets : " + msg);
        }
        public static void Log(string msg)
        {
            if (!log_Enable) return;
            Debug.Log("Assets : " + msg);
        }
        public static void LogError(string err)
        {
            if (!err_Enable) return;
            Debug.LogError("Assets : " + err);
        }


        private static string ToHashString(byte[] bytes)
        {
            byte[] retVal = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetStringHash(string str) => ToHashString(Encoding.UTF8.GetBytes(str));
        public static Type GetAssetType(AssetType assetType, Type type)
        {

            if (type == typeof(UnityEngine.Object))
            {
                switch (assetType)
                {
                    case AssetType.Sprite: return typeof(UnityEngine.Sprite);
                    case AssetType.Shader: return typeof(UnityEngine.Shader);
                    case AssetType.ShaderVariant: return typeof(UnityEngine.ShaderVariantCollection);
                    case AssetType.GameObject: return typeof(UnityEngine.GameObject);
                    case AssetType.VideoClip: return typeof(UnityEngine.Video.VideoClip);
                    case AssetType.AudioClip: return typeof(UnityEngine.AudioClip);
                    case AssetType.TextAsset: return typeof(UnityEngine.TextAsset);
                    case AssetType.Mesh: return typeof(UnityEngine.Mesh);
                    case AssetType.Material: return typeof(UnityEngine.Material);
                    case AssetType.Font: return typeof(UnityEngine.Font);
                    case AssetType.AnimationClip: return typeof(UnityEngine.AnimationClip);
                    case AssetType.AnimatorController:
                    case AssetType.None:
                    case AssetType.Ignore:
                    case AssetType.Directory:
                    case AssetType.Texture:
                    case AssetType.Scene:
                    case AssetType.ScriptObject:
                    default:
                        return type;
                }
            }
            return type;
        }
        public static List<Key> ToKeyList<Key, Value>(Dictionary<Key, Value> dic)
        {
            List<Key> keys = new List<Key>();
            foreach (var item in dic)
            {
                keys.Add(item.Key);
            }
            return keys;
        }
        public static List<Value> ToValueList<Key, Value>(Dictionary<Key, Value> dic)
        {
            List<Value> keys = new List<Value>();
            foreach (var item in dic)
            {
                keys.Add(item.Value);
            }
            return keys;
        }
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
            if (map == null)
                return null;
            Value t;
            if (map.TryGetValue(key, out t))
                return t;
            return null;
        }
        public static string GetFileHash(string path) => AssetsHelper.ExistsFile(path) ? ToHashString(File.ReadAllBytes(path)) : string.Empty;







        public static Operation WriteFile(byte[] bytes, string targetPath, int start, int len) => new WriteFileOperation(targetPath, bytes, start, len);
        public static ReadFileOperation ReadFile(string srcPath, bool async) => new ReadFileOperation(srcPath, async);
        public static string CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static bool ExistsFile(string path) => File.Exists(path);
        public static string[] GetDirectoryFiles(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(x => ToRegularPath(x)).ToArray();
        public static long GetFileLength(string path) => ExistsFile(path) ? new FileInfo(path).Length : 0;
        public static string GetFileName(string path) => Path.GetFileName(path);
        public static string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);
        public static string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        public static string CombinePath(string self, string combine) => Path.Combine(self, combine);
        public static string ToRegularPath(string path) => path.Replace('\\', '/');







        public const string versionExt = ".buffer";
        public static string VersionDataName = $"localversion{versionExt}";
        public static string VersionCollectionName = $"remoteversion{versionExt}";
        public static string GetManifestFileName(string pkgName) => $"manifest_{pkgName}{versionExt}";

        public static BufferWriter WriteBufferObject<T>(T obj) where T : IBufferObject
        {
            BufferWriter writer = new BufferWriter(104857600);
            obj.WriteData(writer);
            return writer;
        }
        public static T ReadBufferObject<T>(byte[] bytes) where T : IBufferObject, new()
        {
            BufferReader reader = new BufferReader(bytes);
            T t = new T();
            t.ReadData(reader);
            return t;
        }
        public static Operation WriteBufferObject<T>(T version, string path) where T : IBufferObject
        {
            var bytes = AssetsHelper.WriteBufferObject(version);
            return AssetsHelper.WriteFile(bytes.buffer, path, 0, bytes.length);
        }

        private static Dictionary<int, Queue<byte[]>> map = new Dictionary<int, Queue<byte[]>>();
        public static byte[] AllocateByteArray(int length)
        {
            var _len = 32;
            while (_len < length)
            {
                _len *= 2;
            }
            length = _len;
            Queue<byte[]> result = GetOrDefaultFromDictionary(map, length);
            if (result == null || result.Count == 0)
                return new byte[length];
            else
                return result.Dequeue();
        }
        public static void RecycleByteArray(byte[] array)
        {
            Array.Clear(array, 0, array.Length);
            Queue<byte[]> result = GetFromDictionary(map, array.Length);
            result.Enqueue(array);
        }
    }
}

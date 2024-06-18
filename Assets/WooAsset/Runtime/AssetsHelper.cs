using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static string StreamBundlePath => $"Assets/StreamingAssets/{buildTarget}";
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

        public static string GetFileHash(string path) => AssetsHelper.ExistsFile(path) ? ToHashString(File.ReadAllBytes(path)) : string.Empty;
        public static T ReadFromBytes<T>(byte[] bytes) where T : IBufferObject, new()
        {
            BufferReader reader = new BufferReader(bytes);
            T t = new T();
            t.ReadData(reader);
            return t;
        }
        public static BufferWriter ObjectToBytes<T>(T obj) where T : IBufferObject
        {
            BufferWriter writer = new BufferWriter(104857600);
            obj.WriteData(writer);
            return writer;
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



        public static Operation WriteFile(byte[] bytes, string targetPath, int start, int len) => new WriteFileOperation(targetPath, bytes, start, len);
        public static ReadFileOperation ReadFile(string srcPath, bool async) => new ReadFileOperation(srcPath, async);
        public static bool ExistsFile(string path) => File.Exists(path);


        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public static string[] GetDirectoryFiles(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(x => ToRegularPath(x)).ToArray();


        public static long GetFileLength(string path) => ExistsFile(path) ? new FileInfo(path).Length : 0;
        public static string GetFileName(string path) => Path.GetFileName(path);
        public static string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);





        public static string CombinePath(string self, string combine) => Path.Combine(self, combine);

        public static string ToRegularPath(string path) => path.Replace('\\', '/');
        public static void LogWarning(string msg) => Debug.LogWarning("Assets : " + msg);
        public static void Log(string msg) => Debug.Log("Assets : " + msg);
        public static void LogError(string err) => Debug.LogError("Assets : " + err);

        public static Type GetAssetType(AssetType assetType, Type type)
        {
            if (type == typeof(UnityEngine.Object))
            {

                switch (assetType)
                {
                    case AssetType.Sprite: return typeof(UnityEngine.Sprite);
                    case AssetType.Shader: return typeof(UnityEngine.Shader);
                    case AssetType.ShaderVariant: return typeof(UnityEngine.ShaderVariantCollection);
                    case AssetType.None:
                    case AssetType.Ignore:
                    case AssetType.Directory:
                    case AssetType.Mesh:
                    case AssetType.Texture:
                    case AssetType.TextAsset:
                    case AssetType.VideoClip:
                    case AssetType.AudioClip:
                    case AssetType.Scene:
                    case AssetType.Material:
                    case AssetType.GameObject:
                    case AssetType.Font:
                    case AssetType.Animation:
                    case AssetType.AnimationClip:
                    case AssetType.AnimatorController:
                    case AssetType.ScriptObject:
                    default:
                        return type;
                }
            }
            return type;
        }

    }
}

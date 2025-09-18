using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        private static readonly Dictionary<string, int> AssetInstanceCache = new Dictionary<string, int>();
        private static readonly Dictionary<string, long> textureSizeCache = new Dictionary<string, long>();
        private static readonly Dictionary<string, Type> AssetTypeCache = new Dictionary<string, Type>();
        private static MethodInfo _GetTextureMemorySizeLong;
        static MethodInfo _GetMainAssetInstanceID;
        public static long GetTextureMemorySizeLong(string path)
        {
            if (_GetTextureMemorySizeLong == null)
            {
                var type = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil");
                _GetTextureMemorySizeLong = type.GetMethod("GetStorageMemorySizeLong", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            }

            if (textureSizeCache.TryGetValue(path, out long cachedType))
                return cachedType;
            var tx = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>(path);
            var newType = (long)_GetTextureMemorySizeLong.Invoke(null, new object[] { tx });
            textureSizeCache[path] = newType;
            return newType;
        }

        public static int GetMainAssetInstanceID(string path)
        {
            if (_GetMainAssetInstanceID == null)
            {
                _GetMainAssetInstanceID = typeof(AssetDatabase).GetMethod("GetMainAssetInstanceID", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (AssetInstanceCache.TryGetValue(path, out int cachedType))
                return cachedType;
            int newType = (int)_GetMainAssetInstanceID.Invoke(null, new object[] { path });
            AssetInstanceCache[path] = newType;
            return newType;
        }
        private static void RemoveCache(string path)
        {
            AssetInstanceCache.Remove(path);
            AssetTypeCache.Remove(path);
            textureSizeCache.Remove(path);
        }
        public static Type GetMainAssetTypeAtPath(string path)
        {
            if (AssetTypeCache.TryGetValue(path, out Type cachedType))
                return cachedType;
            Type newType = AssetDatabase.GetMainAssetTypeAtPath(path);
            AssetTypeCache[path] = newType;
            return newType;
        }

        class CacheModificationProcessor : AssetModificationProcessor
        {
            // 资源即将被创建时调用
            public static void OnWillCreateAsset(string assetPath)
            {
                RemoveCache(assetPath);
            }

            // 资源即将被保存时调用
            public static string[] OnWillSaveAssets(string[] paths)
            {
                foreach (string path in paths)
                {
                    RemoveCache(path);
                }
                return paths; // 必须返回 paths
            }

            // 资源即将被移动时调用
            public static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
            {
                RemoveCache(sourcePath);
                RemoveCache(destinationPath);

                return AssetMoveResult.DidMove; // 返回 DidMove 允许移动
            }

            // 资源即将被删除时调用
            public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
            {
                RemoveCache(assetPath);
                return AssetDeleteResult.DidDelete; // 返回 DidDelete 允许删除
            }

        }
    }
    public partial class AssetsEditorTool : AssetsHelper
    {

        [InitializeOnLoadMethod]
        static void Tool()
        {
            AssetsInternal.AddAssetLife(new LifePart());
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Tool2()
        {
            LifePart.Clear();
            var _op = option;
            AssetsInternal.mode = Activator.CreateInstance(_op.GetAssetModeType()) as IAssetsMode;
            AssetsInternal.SetLocalSaveDir(AssetsEditorTool.EditorSimulatorPath);
            if (_op.server.enable && AssetsInternal.isNormalMode)
                AssetsServer.Run(_op.server.port, ServerDirectory, _op.server.GetSpeed());
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    DownLoader.ClearQueue();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    AssetsServer.Stop();
                    BundleStream.CloseStreams();
                    break;
                default:
                    break;
            }
        }

        private const string DLC = "DLC";
        public static BuildTarget BuildTarget => EditorUserBuildSettings.activeBuildTarget;
        public static string BuildTargetName => buildTarget;
        public static string EditorSimulatorPath => CreateDirectory($"{DLC}/Editor Simulator/{BuildTargetName}");
        public static string OutputPath => CreateDirectory($"{DLC}/Output/{BuildTargetName}");
        public static string HistoryPath => CreateDirectory($"{DLC}/History/{BuildTargetName}");
        public static string EditorPath => CreateDirectory("Assets/Editor");
        public static string ServerDirectory => CreateDirectory($"{DLC}/Server");



        public static AssetsBuildOption option => AssetsScriptableObject.Get<AssetsBuildOption>();
        public static AssetsEditorCache cache => AssetsScriptableObject.Get<AssetsEditorCache>();

        public static event Action onPipelineFinish;

        public class CallPipelineFinishTask : AssetTask
        {
            protected override void OnExecute(AssetTaskContext context)
            {
                onPipelineFinish?.Invoke();
                InvokeComplete();
            }
        }

        public static void MoveFile(string srcPath, string targetPath) => System.IO.File.Move(srcPath, targetPath);
        public static void CopyFile(string srcPath, string targetPath) => System.IO.File.Copy(srcPath, targetPath, true);

        public static byte[] ReadFileSync(string path) => File.ReadAllBytes(path);
        public static void WriteFileSync(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);

        public static void WriteJson<T>(T t, string path)
        {
            WriteFileSync(path, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(t, true)));
        }
        public static T ReadJson<T>(string path)
        {
            return JsonUtility.FromJson<T>(System.Text.Encoding.UTF8.GetString(ReadFileSync(path)));
        }
        internal static void WriteBufferObjectSync<T>(T t, string path) where T : IBufferObject
        {
            var bytes = WriteBufferObject(t);
            var buffer = new byte[bytes.length];
            Array.Copy(bytes.buffer, 0, buffer, 0, buffer.Length);
            WriteFileSync(path, buffer);
        }
        public static void DeleteFile(string path) => File.Delete(path);

        public static string ToAssetsPath(string self) => "Assets" + Path.GetFullPath(self).Substring(Path.GetFullPath(Application.dataPath).Length).Replace("\\", "/");
        public static string[] GetDirectoryEntries(string path) => Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories).Select(x => AssetsEditorTool.ToRegularPath(x)).ToArray();

        public static bool IsDirectory(string path) => Directory.Exists(path);
        public static bool ExistsDirectory(string path) => Directory.Exists(path);
        public static void DeleteDirectory(string path) => Directory.Delete(path, true);
        public static string CombinePath(string path1, string path2, string path3) => Path.Combine(path1, path2, path3);


        [MenuItem(TaskPipelineMenu.SpriteAtlas)]
        public static async Task BuildSpriteAtlas() => await SpriteAtlasTool.Execute(option.spriteAtlas.atlasPaths, option.spriteAtlas.PlatformSetting, option.spriteAtlas.textureSetting, option.spriteAtlas.packSetting);
        [MenuItem(TaskPipelineMenu.ShaderVariant)]
        public static async Task SpriteShaderVariant() => await ShaderVariantTool.Execute(option.shader.OutputDirectory, option.pkgs.SelectMany(x => x.paths).Concat(option.shader.InputDirectory).ToArray());
    }
}

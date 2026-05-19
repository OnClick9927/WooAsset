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
        public static string[] GetAssetDependencies(string path) => cache.GetAssetCache(path).dps;
        public static string GetEditorAssetDataHash(string path) => cache.GetAssetCache(path).hash;
        public static long GetPreviewSizeLong(string path) => cache.GetAssetCache(path).PreviewSize;

        public static int GetMainAssetInstanceID(string path) => cache.GetAssetCache(path).InstanceID;




        private static Dictionary<string, Type> types = new Dictionary<string, Type>();
        public static Type GetTypeByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (types.TryGetValue(name, out var type))
                return type;
            var _type = Types.FirstOrDefault(x => x.FullName == name);
            if (_type == null) return null;
            types.Add(name, _type);
            return _type;
        }
        public static Type GetMainAssetTypeAtPath(string path)
        {
            var _cache = cache.GetAssetCache(path);
            if (_cache.unknownType) return null;
            return GetTypeByName(_cache.type);
        }

    }
    public partial class AssetsEditorTool : AssetsHelper
    {

        [InitializeOnLoadMethod]
        static void Tool()
        {
            foreach (var type in option.encrypt.realTypes)
                AssetsHelper.AddEncrypt((IAssetEncrypt)Activator.CreateInstance(type));

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
            if (_op.mode.ServerEnable && AssetsInternal.isNormalMode)
                AssetsServer.Run(_op.mode.ServerPort, ServerDirectory, _op.mode.GetDownloadSpeed());
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
        internal static AssetsEditorCache cache => AssetsScriptableObject.Get<AssetsEditorCache>();

        public static event Action onPipelineFinish;

        public class CallPipelineFinishTask : AssetTask
        {
            protected override void OnExecute(AssetTaskContext context)
            {
                onPipelineFinish?.Invoke();
                InvokeComplete();
            }
        }
        private static List<Type> _types;

        private static List<Type> Types
        {
            get
            {
                if (_types == null)
                    _types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany((item) => item.GetTypes())
                        .ToList();
                return _types;
            }
        }
        public static IEnumerable<Type> GetSubTypesInAssemblies(Type self)
        {
            if (self.IsInterface)
            {
                return from item in Types
                       where item.GetInterfaces().Contains(self)
                       select item;
            }

            return from item in Types
                   where item.IsSubclassOf(self)
                   select item;
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
        //public static string[] GetDirectoryEntries(string path) => Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories).Select(x => AssetsEditorTool.ToRegularPath(x)).ToArray();

        public static bool IsDirectory(string path) => Directory.Exists(path);
        public static bool ExistsDirectory(string path) => Directory.Exists(path);
        public static void DeleteDirectory(string path) => Directory.Delete(path, true);
        public static string CombinePath(string path1, string path2, string path3) => Path.Combine(path1, path2, path3);


        [MenuItem(TaskPipelineMenu.ResetCache)]
        public static async Task ResetCache()
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(cache));
            //cache.Reset();
            await CallPipelineFinishTask.Execute(new CallPipelineFinishTask(), null);
        }

        [MenuItem(TaskPipelineMenu.SpriteAtlas)]
        public static async Task BuildSpriteAtlas()
        {
            var option = AssetsScriptableObject.Get<SpriteAtlasOption>();

            await SpriteAtlasTool.Execute(option.atlasPaths, option.PlatformSetting, option.textureSetting, option.packSetting);

        }
        [MenuItem(TaskPipelineMenu.ShaderVariant)]
        public static async Task SpriteShaderVariant()
        {
            var _option = AssetsScriptableObject.Get<ShaderOption>();

            await ShaderVariantTool.Execute(_option.OutputDirectory, option.pkgs.SelectMany(x => x.paths).Concat(_option.InputDirectory).ToArray());
        }
        [MenuItem(TaskPipelineMenu.SpriteAtlasOption)]
        public static void SpriteAtlasOption()
        {
            var option = AssetsScriptableObject.Get<SpriteAtlasOption>();
            Selection.activeObject = option;

        }
        [MenuItem(TaskPipelineMenu.ShaderVariantOption)]
        public static void ShaderVariantOption()
        {
            var _option = AssetsScriptableObject.Get<ShaderOption>();
            Selection.activeObject = _option;

        }
    }
}

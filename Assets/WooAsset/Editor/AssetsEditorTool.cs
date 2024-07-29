using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;
using System.Reflection;

namespace WooAsset
{

    partial class AssetsEditorTool : AssetModificationProcessor, IAssetLife<Bundle>, IAssetLife<AssetHandle>
    {
        async void IAssetLife<Bundle>.OnAssetCreate(string path, Bundle asset)
        {
            AssetLife<Bundle> life = new AssetLife<Bundle>()
            {
                asset = asset,
            };
            bundles.Add(asset.bundleName, life);
            await asset;
            life.assetLength = asset.length;
            onAssetLifeChange?.Invoke();
        }
        void IAssetLife<Bundle>.OnAssetRetain(Bundle asset, int count) => onAssetLifeChange?.Invoke();
        void IAssetLife<Bundle>.OnAssetRelease(Bundle asset, int count) => onAssetLifeChange?.Invoke();
        void IAssetLife<Bundle>.OnAssetUnload(string path, Bundle asset)
        {
            bundles.Remove(asset.bundleName);
            onAssetLifeChange?.Invoke();
        }
        async void IAssetLife<AssetHandle>.OnAssetCreate(string path, AssetHandle asset)
        {
            var data = asset.data;
            var life = new AssetLife<AssetHandle>()
            {
                asset = asset,
                tags = Assets.GetAssetTags(path),
                assetType = AssetsInternal.GetAssetType(path).ToString(),
            };
            assets.Add(path, life);
            onAssetLifeChange?.Invoke();
            await asset;
            onAssetLifeChange?.Invoke();
        }
        void IAssetLife<AssetHandle>.OnAssetRelease(AssetHandle asset, int count) => onAssetLifeChange?.Invoke();
        void IAssetLife<AssetHandle>.OnAssetRetain(AssetHandle asset, int count) => onAssetLifeChange?.Invoke();
        void IAssetLife<AssetHandle>.OnAssetUnload(string path, AssetHandle asset)
        {
            assets.Remove(path);
            onAssetLifeChange?.Invoke();

        }

        public class AssetLife<T> where T : AssetOperation
        {
            public T asset;
            public long assetLength;
            public IReadOnlyList<string> tags;
            public string assetType;
        }
        public static Dictionary<string, AssetLife<Bundle>> bundles = new Dictionary<string, AssetLife<Bundle>>();
        public static Dictionary<string, AssetLife<AssetHandle>> assets = new Dictionary<string, AssetLife<AssetHandle>>();
        private static AssetsEditorTool ins = new AssetsEditorTool();
        public static event Action onAssetLifeChange;

        [InitializeOnLoadMethod]
        static void Tool()
        {
            AssetsInternal.AddAssetLife(ins);
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Tool2()
        {
            bundles.Clear();
            assets.Clear();
            var _op = option;
            AssetsInternal.mode = Activator.CreateInstance(_op.GetAssetModeType()) as IAssetsMode;
            AssetsInternal.SetLocalSaveDir(AssetsEditorTool.EditorSimulatorPath);
            if (_op.enableServer && AssetsInternal.mode is NormalAssetsMode)
                AssetsServer.Run(_op.serverPort, ServerDirectory);
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
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

    }
    public partial class AssetsEditorTool
    {
        private const string DLC = "DLC";
        public static BuildTarget BuildTarget => EditorUserBuildSettings.activeBuildTarget;
        public static string BuildTargetName => AssetsHelper.buildTarget;
        public static string EditorSimulatorPath
        {
            get
            {
                string path = $"{DLC}/Editor Simulator/{BuildTargetName}";
                AssetsHelper.CreateDirectory(path);
                return path;
            }
        }
        public static string OutputPath
        {
            get
            {
                string path = $"{DLC}/Output/{BuildTargetName}";
                AssetsHelper.CreateDirectory(path);
                return path;
            }
        }
        public static string HistoryPath
        {
            get
            {
                string path = $"{DLC}/History/{BuildTargetName}";

                AssetsHelper.CreateDirectory(path);
                return path;
            }
        }
        public static string EditorPath
        {
            get
            {

                string path = "Assets/Editor";
                AssetsHelper.CreateDirectory(path);

                return path;
            }
        }
        public static string ServerDirectory
        {
            get
            {
                string path = $"{DLC}/Server";
                AssetsHelper.CreateDirectory(path);
                return path;
            }
        }


        private static AssetsBuildOption _option;
        private static AssetsEditorCache _cache;
        public static AssetsBuildOption option
        {
            get
            {
                if (!_option)
                {
                    _option = AssetsScriptableObject.Load<AssetsBuildOption>();
                    _option.OnEnable();
                }
                return _option;
            }
        }
        public static AssetsEditorCache cache
        {
            get
            {
                if (!_cache)
                    _cache = AssetsScriptableObject.Load<AssetsEditorCache>();
                return _cache;
            }
        }

        public static event Action onPipelineFinish;

        public class CallPipelineFinishTask : AssetTask
        {
            protected override void OnExecute(AssetTaskContext context)
            {
                onPipelineFinish?.Invoke();
                InvokeComplete();
            }
        }




        public static T CreateScriptableObject<T>(string savePath) where T : ScriptableObject
        {
            ScriptableObject sto = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(sto, savePath);
            EditorUtility.SetDirty(sto);
            AssetDatabase.ImportAsset(savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return Load<T>(savePath);
        }
        public static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
        public static void Update<T>(T t) where T : Object
        {
            EditorApplication.delayCall += delegate ()
            {
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            };
        }

        public static void MoveFile(string srcPath, string targetPath) => System.IO.File.Move(srcPath, targetPath);
        public static void CopyFile(string srcPath, string targetPath) => System.IO.File.Copy(srcPath, targetPath);

        public static Operation WriteObject<T>(T t, string path) where T : IBufferObject
        {
            var bytes = AssetsHelper.ObjectToBytes(t);
            return AssetsHelper.WriteFile(bytes.buffer, path, 0, bytes.length);
        }

        public static void DeleteFile(string path) => File.Delete(path);

        public static string ToAssetsPath(string self) => "Assets" + Path.GetFullPath(self).Substring(Path.GetFullPath(Application.dataPath).Length).Replace("\\", "/");
        public static string[] GetDirectoryEntries(string path) => Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories).Select(x => AssetsHelper.ToRegularPath(x)).ToArray();
        public static string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        public static Operation WriteStream(string srcPath, Stream target) => new CopyFileStreamOperation(srcPath, target);

        public static bool IsDirectory(string path) => Directory.Exists(path);
        public static bool ExistsDirectory(string path) => Directory.Exists(path);
        public static void DeleteDirectory(string path) => Directory.Delete(path, true);
        public static string CombinePath(string path1, string path2, string path3) => Path.Combine(path1, path2, path3);

        private static MethodInfo _GetTextureMemorySizeLong;
        public static long GetTextureMemorySizeLong(string path)
        {
            if (_GetTextureMemorySizeLong == null)
            {
                var type = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil");
                _GetTextureMemorySizeLong = type.GetMethod("GetStorageMemorySizeLong", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            }
            var tx = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>(path);
            return (long)_GetTextureMemorySizeLong.Invoke(null, new object[] { tx });
        }

        [MenuItem(TaskPipelineMenu.SpriteAtlas)]
        public static async Task BuildSpriteAtlas() => await SpriteAtlasTool.Execute();
        [MenuItem(TaskPipelineMenu.ShaderVariant)]
        public static async Task SpriteShaderVariant() => await ShaderVariantTool.Execute();

    }
}

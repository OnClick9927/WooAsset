using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WooAsset
{

    partial class AssetsEditorTool : UnityEditor.AssetModificationProcessor, IAssetLife<Bundle>, IAssetLife<AssetHandle>
    {
        async void IAssetLife<Bundle>.OnAssetCreate(string path, Bundle asset)
        {
            AssetLife<Bundle> life = new AssetLife<Bundle>()
            {
                asset = asset,
            };
            bundles.Add(asset.bundleName, life);
            await asset;
            life.assetLength = asset.assetLength;
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

            var life = new AssetLife<AssetHandle>()
            {
                asset = asset,
                tags = AssetsInternal.GetAssetTags(path),
                assetType = AssetsInternal.GetAssetType(path).ToString(),
            };
            assets.Add(path, life);
            onAssetLifeChange?.Invoke();
            await asset;

            life.assetLength = asset.assetLength;
            onAssetLifeChange?.Invoke();
        }
        void IAssetLife<AssetHandle>.OnAssetRelease(AssetHandle asset, int count) => onAssetLifeChange?.Invoke();
        void IAssetLife<AssetHandle>.OnAssetRetain(AssetHandle asset, int count) => onAssetLifeChange?.Invoke();
        void IAssetLife<AssetHandle>.OnAssetUnload(string path, AssetHandle asset)
        {
            assets.Remove(path);
            onAssetLifeChange?.Invoke();

        }

        public class AssetLife<T> where T : IAsset
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
            AssetTaskRunner.PreviewAssets();
            AssetsInternal.mode = Activator.CreateInstance(option.GetAssetModeType()) as IAssetMode;
            AssetsInternal.localSaveDir = AssetsEditorTool.outputPath;
            if (option.enableServer && AssetsInternal.mode is NormalAssetMode)
                AssetsServer.Run(option.serverPort, option.serverDirectory);
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
                    break;
                default:
                    break;
            }
        }
        static string[] OnWillSaveAssets(string[] paths)
        {
            bool go = false;
            foreach (string path in paths)
            {
                if (!IsIgnorePath(path))
                {
                    go = true;
                    break;
                }
            }
            if (go)

                AssetTaskRunner.PreviewAssets();

            return paths;
        }
        static void OnWillCreateAsset(string path)
        {
            if (IsIgnorePath(path)) return;
            AssetTaskRunner.PreviewAssets();
        }

        //--监听“资源即将被移动”事件
        static AssetMoveResult OnWillMoveAsset(string assetPath, string newPath)
        {
            if (!IsIgnorePath(assetPath))
            {
                AssetTaskRunner.PreviewAssets();
            }
            return AssetMoveResult.DidNotMove;
        }
        //--监听“资源即将被删除”事件
        static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
        {
            if (!IsIgnorePath(assetPath))
            {
                AssetTaskRunner.PreviewAssets();
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
    public partial class AssetsEditorTool
    {
        public static BuildTarget buildTarget
        {
            get { return EditorUserBuildSettings.activeBuildTarget; }
        }
        public static string buildTargetName => AssetsInternal.buildTarget;
        public static string outputPath
        {
            get
            {
                string path = "DLC/";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = AssetsInternal.CombinePath(path, buildTargetName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        public static string historyPath
        {
            get
            {
                string path = "DLC/History/";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = AssetsInternal.CombinePath(path, buildTargetName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        public static string editorPath
        {
            get
            {
                string path = "Assets/Editor";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
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
                {
                    _cache = AssetsScriptableObject.Load<AssetsEditorCache>();
                }
                return _cache;
            }
        }


        public static bool IsIgnorePath(string path)
        {
            var list = AssetsInternal.ToRegularPath(path).Split('/').ToList();
            if (!list.Contains("Assets") || list.Contains("Editor") || list.Contains("Resources")) return true;
            for (int i = 0; i < option.ignoreFileExtend.Count; i++)
            {
                if (path.EndsWith(option.ignoreFileExtend[i]))
                {
                    return true;
                }
            }
            return false;
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

        public static string ToAssetsPath(string self)
        {
            string assetRootPath = Path.GetFullPath(Application.dataPath);
            return "Assets" + Path.GetFullPath(self).Substring(assetRootPath.Length).Replace("\\", "/");
        }
        public static bool IsDirectory(string path)
        {
            return Directory.Exists(path);
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
        public static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public static void Update<T>(T t) where T : Object
        {
            EditorApplication.delayCall += delegate ()
            {
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            };
        }
    }
}

using UnityEditor;
using System.Collections.Generic;
using System;
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
                tags = Assets.GetAssetTags(path),
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
            AssetsInternal.mode = Activator.CreateInstance(option.GetAssetModeType()) as IAssetMode;
            AssetsInternal.SetLocalSaveDir(AssetsEditorTool.outputPath);
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
                AssetsHelper.CreateDirectory(path);
                path = AssetsHelper.CombinePath(path, buildTargetName);
                AssetsHelper.CreateDirectory(path);
                return path;
            }
        }
        public static string historyPath
        {
            get
            {
                string path = "DLC/History/";
                AssetsHelper.CreateDirectory(path);
                path = AssetsHelper.CombinePath(path, buildTargetName);
                AssetsHelper.CreateDirectory(path);
                return path;
            }
        }
        public static string editorPath
        {
            get
            {
                string path = "Assets/Editor";
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
                {
                    _cache = AssetsScriptableObject.Load<AssetsEditorCache>();
                }
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

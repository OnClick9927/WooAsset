using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class EditorBundle : Bundle
        {
            public class EditorAssetRequest : AssetRequest
            {
                private Object[] _allAssets;
                private Object _asset;

                public EditorAssetRequest(Object[] allAssets, Object asset)
                {
                    _allAssets = allAssets;
                    InvokeComplete();
                    _asset = asset;
                }

                public override Object asset => _asset;

                public override UnityEngine.Object[] allAssets => _allAssets;

                public override float progress => 1;
            }

            public EditorBundle(BundleLoadArgs loadArgs) : base(loadArgs)
            {
            }
            public override float progress => 1;
            public override bool async => false;
            protected override void OnLoad()
            {
                SetResult(null);
            }
            protected override void OnUnLoad() { }
            protected override long ProfilerAsset(AssetBundle value) {
                return cache.GetBundleGroupByBundleName(this.bundleName).length;
            }
            public override Object[] LoadAsset(string path, Type type)
            {
                return AssetDatabase.LoadAllAssetsAtPath(path);
            }
            public override AssetRequest LoadAssetAsync(string path, Type type)
            {
                var _allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                var result = AssetDatabase.LoadAssetAtPath(path, type);
                return new EditorAssetRequest(_allAssets, result);
            }
            public override AsyncOperation LoadSceneAsync(string path, LoadSceneParameters parameters)
            {
                return EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
            public override Scene LoadScene(string path, LoadSceneParameters parameters)
            {
                return EditorSceneManager.LoadSceneInPlayMode(path, parameters);
            }
        
        }

    }

}

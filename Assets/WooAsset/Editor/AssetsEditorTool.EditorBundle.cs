using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace WooAsset
{
    partial class AssetsEditorTool
    {
        class EditorBundle : Bundle
        {
            private class EditorAssetRequest : AssetRequest
            {
                private Object[] _allAssets;
                private Object _asset;

                public EditorAssetRequest(Object[] allAssets, Object asset)
                {
                    _allAssets = allAssets;
                    _asset = asset;
                    InvokeComplete();
                }

                public override Object asset => _asset;

                public override UnityEngine.Object[] allAssets => _allAssets;

                public override float progress => 1;
            }

            public EditorBundle(BundleLoadArgs loadArgs) : base(loadArgs) { }
            public override float progress => 1;
            public override bool async => false;
            protected override void OnLoad() => SetResult(null);
            protected override void OnUnLoad() { }

            public override RawObject LoadRawObject(string path) => RawObject.Create(path, AssetsHelper.ReadFile(path, false).bytes);

            public override Object[] LoadAssetWithSubAssets(string path, Type type)
            {
                var _allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                var result = AssetDatabase.LoadAssetAtPath(path, type);
                for (int i = 0; i < _allAssets.Length; i++)
                {
                    if (result == _allAssets[i])
                    {
                        var tmp = _allAssets[i];
                        _allAssets[i] = _allAssets[0];
                        _allAssets[0] = tmp;
                    }
                }
                return _allAssets;
            }
            public override AssetRequest LoadAssetWithSubAssetsAsync(string path, Type type) => new EditorAssetRequest(AssetDatabase.LoadAllAssetsAtPath(path), AssetDatabase.LoadAssetAtPath(path, type));
            public override AssetRequest LoadAssetAsync(string name, Type type) => new EditorAssetRequest(null, AssetDatabase.LoadAssetAtPath(name, type));
            public override Object LoadAsset(string name, Type type) => AssetDatabase.LoadAssetAtPath(name, type);
            public override AsyncOperation LoadSceneAsync(string path, LoadSceneParameters parameters) => EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            public override Scene LoadScene(string path, LoadSceneParameters parameters) => EditorSceneManager.LoadSceneInPlayMode(path, parameters);
        }

    }

}

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
        public class EditorBundle : Bundle
        {
            private class EditorAssetRequest : AssetRequest
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

            public override RawObject LoadRawObject(string path)
            {
                RawObject rawObject = RawObject.Create(path);
                var reader = AssetsHelper.ReadFile(path, false);
                rawObject.bytes = reader.bytes;
                return rawObject;
            }

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
            public override AssetRequest LoadAssetWithSubAssetsAsync(string path, Type type)
            {
                var _allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                var result = AssetDatabase.LoadAssetAtPath(path, type);
                return new EditorAssetRequest(_allAssets, result);
            }


            public override AssetRequest LoadAssetAsync(string name, Type type)
            {
                var result = AssetDatabase.LoadAssetAtPath(name, type);
                return new EditorAssetRequest(null, result);
            }
            public override Object LoadAsset(string name, Type type) => AssetDatabase.LoadAssetAtPath(name, type);
            public override AsyncOperation LoadSceneAsync(string path, LoadSceneParameters parameters) => EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            public override Scene LoadScene(string path, LoadSceneParameters parameters) => EditorSceneManager.LoadSceneInPlayMode(path, parameters);
        }

    }

}

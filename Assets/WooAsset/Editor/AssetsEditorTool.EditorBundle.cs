using System;
//using System.Threading;
using System.Threading.Tasks;
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
                private string path;

                public EditorAssetRequest(string path, Object[] allAssets, Object asset)
                {
                    _allAssets = allAssets;
                    _asset = asset;
                    this.path = path;
                    DelayComplete();
                }
                private async void DelayComplete()
                {
                    var wait_time = GetWaitTime(AssetsEditorTool.GetPreviewSizeLong(path));
                    await Task.Delay((int)wait_time);
                    InvokeComplete();

                }
                public override Object asset => _asset;

                public override UnityEngine.Object[] allAssets => _allAssets;

                public override float progress => 1;
            }

            public enum Mode
            {
                AssetDataBase,
                Rude,
            }
            private Mode mode;
            public EditorBundle(BundleLoadArgs loadArgs, Mode mode) : base(loadArgs)
            {
                this.mode = mode;
            }
            public override float progress => 1;
            public override bool async => false;
            static int GetWaitTime(long length)
            {
                return (int)(length / option.mode.GetEditorReadSpeed() / 1f);
            }
            static void ThreadSleep(string path)
            {
                var length = AssetsEditorTool.GetPreviewSizeLong(path);
                var time = GetWaitTime(length);
                System.Threading.Thread.Sleep(time);
            }
            protected override async void OnLoad()
            {
                if (mode == Mode.AssetDataBase)
                {
                    var wait_time = GetWaitTime(rawLength);
                    await Task.Delay((int)wait_time);
                }

                SetResult(null);

            }
            //protected override long ProfileAsset(AssetBundle value)
            //{
            //    return rawLength;
            //    //var find = cache.ForceGetBundleGroupByBundleName(this.bundleName);
            //    //return find != null ? (int)find.length : 0;
            //}

            public override RawObject LoadRawObject(string path)
            {
                if (mode == Mode.Rude)
                    ThreadSleep(path);


                return RawObject.Create(path, AssetsEditorTool.ReadFileSync(path));
            }

            public override Object[] LoadAssetWithSubAssets(string path, Type type)
            {
                ThreadSleep(path);
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
            internal override AssetRequest LoadAssetWithSubAssetsAsync(string path, Type type) => new EditorAssetRequest(path, AssetDatabase.LoadAllAssetsAtPath(path), AssetDatabase.LoadAssetAtPath(path, type));
            internal override AssetRequest LoadAssetAsync(string name, Type type) => new EditorAssetRequest(name, null, AssetDatabase.LoadAssetAtPath(name, type));
            public override Object LoadAsset(string name, Type type)
            {
                ThreadSleep(name);

                return AssetDatabase.LoadAssetAtPath(name, type);
            }

            public override AsyncOperation LoadSceneAsync(string path, LoadSceneParameters parameters)
            {
                ThreadSleep(path);

                return EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }

            public override Scene LoadScene(string path, LoadSceneParameters parameters)
            {
                ThreadSleep(path);

                return EditorSceneManager.LoadSceneInPlayMode(path, parameters);
            }
        }

    }

}

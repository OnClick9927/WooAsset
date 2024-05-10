using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace WooAsset
{
    public class Bundle : AssetOperation<AssetBundle>
    {
        private enum BundleLoadType
        {
            FromFile,
            FromRequest,
        }
        private bool _async = false;
        private string _path;
        private BundleLoadType type;
        private IAssetStreamEncrypt encrypt => loadArgs.encrypt;
        public override bool async => _async;
        protected BundleLoadArgs loadArgs;

        public string bundleName => loadArgs.bundleName;

        private AssetBundleCreateRequest loadOp;
        private Operation downloader;
        public Bundle(BundleLoadArgs loadArgs)
        {
            this.loadArgs = loadArgs;
            _async = loadArgs.async;
            type = AssetsInternal.GetBundleAwalysFromWebRequest() ? BundleLoadType.FromRequest : BundleLoadType.FromFile;
            _path = AssetsInternal.GetBundleLocalPath(bundleName);
            if (type == BundleLoadType.FromFile && !AssetsHelper.ExistsFile(_path))
            {
                type = BundleLoadType.FromRequest;
                _async = true;
            }

        }

        public override float progress
        {
            get
            {
                if (async)
                {
                    if (type == BundleLoadType.FromRequest)
                    {
                        if (isDone) return 1;
                        if (loadOp == null) return downloader.progress * 0.5f;
                        return downloader.progress * 0.5f + loadOp.progress * 0.5f;
                    }
                    return isDone ? 1 : (loadOp == null) ? 0 : loadOp.progress;
                }
                return isDone ? 1 : 0;
            }
        }

        private long _length;
        public long length => _length;
        protected virtual long ProfilerAsset(AssetBundle value)
        {
            return Profiler.GetRuntimeMemorySizeLong(value);
        }
        private async void LoadBundle(byte[] buffer)
        {
            buffer = EncryptBuffer.Decode(loadArgs.bundleName, buffer, encrypt);
            if (async)
            {
                loadOp = AssetBundle.LoadFromMemoryAsync(buffer);
                await this.loadOp;
                if (loadOp.assetBundle == null)
                {
                    SetErr($"Can not Load Bundle {bundleName}");
                }
                SetResult(loadOp.assetBundle);
            }
            else
            {
                AssetBundle result = AssetBundle.LoadFromMemory(buffer);
                if (result == null)
                {
                    SetErr($"Can not Load Bundle {bundleName}");
                }
                SetResult(result);
            }
        }

        protected override void SetResult(AssetBundle value)
        {
            _length = ProfilerAsset(value);
            base.SetResult(value);
        }
        private async void LoadFromStream()
        {
            BundleStream bs = new BundleStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, bundleName, encrypt);
            if (async)
            {
                loadOp = AssetBundle.LoadFromStreamAsync(bs);
                await this.loadOp;
                if (loadOp.assetBundle == null)
                {
                    SetErr($"Can not Load Bundle {bundleName}");
                }
                SetResult(loadOp.assetBundle);
            }
            else
            {
                AssetBundle result = AssetBundle.LoadFromStream(bs);
                if (result == null)
                {
                    SetErr($"Can not Load Bundle {bundleName}");
                }
                SetResult(result);
            }

        }
        protected override async void OnLoad()
        {
            if (type == BundleLoadType.FromFile)
            {
                LoadFromStream();
                //var reader = await AssetsHelper.ReadFile(_path, async);
                //LoadBundle(reader.bytes);
            }
            else
            {
                if (encrypt is NoneAssetStreamEncrypt)
                {
                    var downloader = AssetsInternal.DownLoadBundle(bundleName);
                    this.downloader = downloader;
                    await downloader;
                    if (!downloader.isErr)
                    {
                        SetResult(downloader.bundle);
                    }
                    else
                    {
                        SetErr(downloader.error);
                        SetResult(null);
                    }
                }
                else
                {
                    var downloader = AssetsInternal.DownloadVersion(bundleName);
                    this.downloader = downloader;
                    await downloader;
                    if (!downloader.isErr)
                    {
                        byte[] buffer = downloader.data;
                        //await downloader.SaveBundleToLocal();
                        LoadBundle(buffer);
                    }
                    else
                    {
                        SetErr(downloader.error);
                        SetResult(null);
                    }
                }

            }
        }

        protected override void OnUnLoad()
        {
            value.Unload(true);
            //Resources.UnloadUnusedAssets();
        }

        public virtual AssetRequest LoadAssetAsync(string name, Type type)
        {
            return new RuntimeAssetRequest(value.LoadAssetWithSubAssetsAsync(name, type));
        }
        public virtual UnityEngine.Object[] LoadAsset(string name, Type type)
        {
            return value.LoadAssetWithSubAssets(name, type);
        }

        public virtual Scene LoadScene(string path, LoadSceneParameters parameters) => SceneManager.LoadScene(AssetsHelper.GetFileNameWithoutExtension(path), parameters);
        public AsyncOperation UnLoadScene(string path, UnloadSceneOptions op) => SceneManager.UnloadSceneAsync(AssetsHelper.GetFileNameWithoutExtension(path), op);
        public virtual AsyncOperation LoadSceneAsync(string path, LoadSceneParameters parameters) => SceneManager.LoadSceneAsync(AssetsHelper.GetFileNameWithoutExtension(path), parameters);
    }

}

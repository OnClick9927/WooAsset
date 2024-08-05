using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace WooAsset
{

    public class Bundle : AssetOperation
    {
        private enum BundleLoadType
        {
            FromFile,
            FromRequest,
        }
        private bool _async = false;
        private string _path;
        private BundleLoadType type;
        private IAssetEncrypt encrypt => loadArgs.encrypt;
        public override bool async => _async;
        protected BundleLoadArgs loadArgs;

        public string bundleName => loadArgs.bundleName;
        public bool raw => loadArgs.data.raw;
        public long rawLength => loadArgs.data.length;


        private AssetBundleCreateRequest loadOp;
        private Operation downloader;
        public AssetBundle value { get; private set; }
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
                if (isDone) return 1;
                if (async)
                {

                    if (type == BundleLoadType.FromRequest)
                    {
                        if (downloader == null) return 0;
                        if (raw)
                            return downloader.progress;
                        else
                        {
                            if (loadOp == null) return (downloader.progress + dependence.progress) * 0.5f;
                            return (downloader.progress + dependence.progress) * 0.5f + loadOp.progress * 0.5f;
                        }
                    }
                    if (raw)
                        return rawProgress;
                    return loadOp == null ? 0 : loadOp.progress;
                }
                return 0;
            }
        }

        private long _length;
        public long length => _length;
        private long ProfilerAsset(AssetBundle value)
        {
            if (value == null) return 0;
            return Profiler.GetRuntimeMemorySizeLong(value);
        }
        private async void LoadBundle(byte[] buffer)
        {
            buffer = EncryptBuffer.Decode(bundleName, buffer, encrypt);
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

        protected async void SetResult(AssetBundle value)
        {
            if (raw)
                _length = rawLength;
            else
                _length = ProfilerAsset(value);
            await dependence;
            this.value = value;
            InvokeComplete();
        }
        private RawObject rawObject;
        private float rawProgress;
        FileStream filestream;
        private async void LoadFromStream()
        {
            filestream = new BundleStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, bundleName, encrypt);

            long len = filestream.Length;

            if (raw)
            {
                rawObject = RawObject.Create(_path, new byte[len]);
            }



            if (async)
            {
                if (raw)
                {
                    int n = 1024 * 8;
                    int offset = 0;
                    var bytes = rawObject.bytes;
                    long last = len;
                    while (last > 0)
                    {
                        var read = filestream.Read(bytes, offset, (int)Math.Min(n, last));
                        offset += read;
                        last -= read;
                        rawProgress = offset / (float)len;
                        if (last <= 0) break;
                        await new YieldOperation();
                    }
                    SetResult(null);
                }
                else
                {

                    loadOp = AssetBundle.LoadFromStreamAsync(filestream);
                    await this.loadOp;
                    if (loadOp.assetBundle == null)
                    {
                        SetErr($"Can not Load Bundle {bundleName}");
                    }
                    Debug.Log(loadOp.assetBundle);
                    SetResult(loadOp.assetBundle);
                }
            }
            else
            {
                if (raw)
                {
                    filestream.Read(rawObject.bytes, 0, (int)length);
                    SetResult(null);
                }
                else
                {
                    AssetBundle result = AssetBundle.LoadFromStream(filestream);
                    if (result == null)
                        SetErr($"Can not Load Bundle {bundleName}");
                    SetResult(result);
                }
            }
        }
        private Operation dependence => loadArgs.dependence;
        protected override async void OnLoad()
        {


            if (type == BundleLoadType.FromFile)
            {
                LoadFromStream();
            }
            else
            {
                if (raw || (!raw && encrypt is NoneAssetStreamEncrypt))
                {
                    var downloader = AssetsInternal.DownloadRawBundle(AssetsInternal.GetVersion(), bundleName);
                    this.downloader = downloader;
                    await downloader;
                }

                if (raw)
                {
                    var downloader = AssetsInternal.DownloadRawBundle(AssetsInternal.GetVersion(), bundleName);
                    this.downloader = downloader;
                    await downloader;
                    rawObject = RawObject.Create(_path, EncryptBuffer.Decode(bundleName, downloader.data, encrypt));
                    SetResult(null);
                }
                else
                {
                    if (encrypt is NoneAssetStreamEncrypt)
                    {
                        var downloader = AssetsInternal.DownLoadBundle(AssetsInternal.GetVersion(), bundleName);
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
                        var downloader = AssetsInternal.DownloadRawBundle(AssetsInternal.GetVersion(), bundleName);
                        this.downloader = downloader;
                        await downloader;
                        if (!downloader.isErr)
                        {
                            byte[] buffer = downloader.data;
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
        }

        protected override void OnUnLoad()
        {
            if (value != null)
                value.Unload(true);
            if (filestream != null)
            {
                filestream.Dispose();
                filestream = null;
            }

        }

        public virtual RawObject LoadRawObject(string path) => rawObject;
        public virtual AssetRequest LoadAssetWithSubAssetsAsync(string name, Type type) => new RuntimeAssetRequest(value.LoadAssetWithSubAssetsAsync(name, type));
        public virtual UnityEngine.Object[] LoadAssetWithSubAssets(string name, Type type) => value.LoadAssetWithSubAssets(name, type);
        public virtual AssetRequest LoadAssetAsync(string name, Type type) => new RuntimeAssetRequest(value.LoadAssetAsync(name, type));
        public virtual UnityEngine.Object LoadAsset(string name, Type type) => value.LoadAsset(name, type);
        public virtual Scene LoadScene(string path, LoadSceneParameters parameters) => SceneManager.LoadScene(AssetsHelper.GetFileNameWithoutExtension(path), parameters);
        public AsyncOperation UnloadSceneAsync(string path, UnloadSceneOptions op) => SceneManager.UnloadSceneAsync(AssetsHelper.GetFileNameWithoutExtension(path), op);
        public virtual AsyncOperation LoadSceneAsync(string path, LoadSceneParameters parameters) => SceneManager.LoadSceneAsync(AssetsHelper.GetFileNameWithoutExtension(path), parameters);
    }

}

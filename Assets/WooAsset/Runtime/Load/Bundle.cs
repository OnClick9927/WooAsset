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
        private long _length;
        private BundleLoadType type;
        private IAssetEncrypt encrypt => loadArgs.encrypt;
        public override bool async => _async;
        protected BundleLoadArgs loadArgs;
        public string bundleName => loadArgs.bundleName;
        public bool raw => loadArgs.data.raw;
        public long rawLength => loadArgs.data.length;
        protected CompressType compress => loadArgs.data.compress;
        public long length => _length;
        private Operation dependence => loadArgs.dependence;

        private AssetBundleCreateRequest loadOp;
        private Operation downloader;
        private Operation bytes_op;

        private RawObject rawObject;

        FileStream filestream;
        public AssetBundle value { get; private set; }
        public Bundle(BundleLoadArgs loadArgs)
        {
            this.loadArgs = loadArgs;
            _async = loadArgs.async;
            type = AssetsInternal.GetBundleAlwaysFromWebRequest() ? BundleLoadType.FromRequest : BundleLoadType.FromFile;
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
                    var op = bytes_op != null ? bytes_op : downloader;
                    return op == null ? loadOp.progress : op.progress;
                }
                return 0;
            }
        }




        private long ProfilerAsset(AssetBundle value)
        {
            if (value == null) return 0;
            return Profiler.GetRuntimeMemorySizeLong(value);
        }
        private async void LoadFromBytes(Operation op)
        {
            bytes_op = op;
            byte[] buffer = null;
            await op;
            if (op is ReadFileOperation)
                buffer = (op as ReadFileOperation).bytes;
            else if (op is DownLoader)
                buffer = (op as DownLoader).data;
            if (op.isErr)
            {
                SetErr(op.error);
                SetResult(null);
                return;
            }


            buffer = EncryptBuffer.Decode(bundleName, buffer, encrypt);
            if (raw)
            {
                rawObject = RawObject.Create(_path, buffer);
                SetResult(null);
            }
            else
            {
                if (async)
                {
                    loadOp = AssetBundle.LoadFromMemoryAsync(buffer);
                    await this.loadOp;
                    SetResult(loadOp.assetBundle);
                }
                else
                    SetResult(AssetBundle.LoadFromMemory(buffer));
            }
        }
        protected async void SetResult(AssetBundle value)
        {
#if UNITY_EDITOR
            if (GetType() == typeof(Bundle))
#endif
                if (!raw)
                {
                    if (value == null)
                    {
                        SetErr($"Can not Load Bundle {bundleName}");
                    }
                }


            if (raw)
                _length = rawLength;
            else
                _length = ProfilerAsset(value);


            await dependence;
            this.value = value;
            InvokeComplete();
        }

        private async void LoadFromStream()
        {
            if (raw || (compress != CompressType.LZMA && !(encrypt is NoneAssetStreamEncrypt)))
                LoadFromBytes(AssetsHelper.ReadFile(_path, async));
            else
            {
                filestream = new BundleStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, bundleName, encrypt);
                long len = filestream.Length;
                if (async)
                {
                    loadOp = AssetBundle.LoadFromStreamAsync(filestream);
                    await this.loadOp;
                    SetResult(loadOp.assetBundle);
                }
                else
                    SetResult(AssetBundle.LoadFromStream(filestream));
            }
        }
        private async void LoadFromRequest()
        {
            if (raw || (!raw && !(encrypt is NoneAssetStreamEncrypt)))
                LoadFromBytes(AssetsInternal.DownloadRawBundle(AssetsInternal.GetVersion(), bundleName));
            else
            {
                var downloader = AssetsInternal.DownLoadBundle(AssetsInternal.GetVersion(), bundleName);
                this.downloader = downloader;
                await downloader;
                SetResult(downloader.bundle);
            }
        }
        protected override void OnLoad()
        {
            if (type == BundleLoadType.FromFile)
                LoadFromStream();
            else
                LoadFromRequest();
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

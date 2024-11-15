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
        public BundleLoadArgs loadArgs { get; private set; }
        public string bundleName => loadArgs.bundleName;
        public bool raw => loadArgs.data.raw;
        public long rawLength => loadArgs.data.length;
        protected CompressType compress => loadArgs.data.compress;
        public long length => _length;
        private Operation dependence => loadArgs.dependence;



        private RawObject rawObject { get { return mode.rawObject; } }

        public AssetBundle value { get; private set; }

        private Mode mode;
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

        private abstract class Mode
        {
            public virtual float progress { get; }
            private Bundle bundle;
            public bool raw => bundle.raw;
            public IAssetEncrypt encrypt => bundle.encrypt;
            protected CompressType compress => bundle.compress;
            public string path => bundle._path;
            public bool async => bundle._async;
            public string bundleName => bundle.bundleName;
            public Operation dependence => bundle.dependence;
            public RawObject rawObject { get; private set; }

            public float bytesProgress
            {
                get
                {
                    if (loadOp == null)
                        return bytes_op.progress * 0.5f;
                    return loadOp.progress * 0.5f + 0.5f;
                }
            }

            private Operation bytes_op;
            private AssetBundleCreateRequest loadOp;
            private AssetBundle assetbundle;
            protected Mode(Bundle bundle)
            {
                this.bundle = bundle;
            }

            private bool _setResult = false;
            public void Update()
            {
                OnUpdate();
                if (!_setResult) return;
                if (!dependence.isDone) return;
                bundle.SetResult(assetbundle);

            }
            protected abstract void OnUpdate();
            protected void End(AssetBundle value)
            {
                _setResult = true;
                assetbundle = value;
            }

            protected void LoadFromBytes(Operation op)
            {
                if (!op.isDone) return;
                bytes_op = op;
                byte[] buffer = null;
                if (op is ReadFileOperation)
                    buffer = (op as ReadFileOperation).bytes;
                else if (op is DownLoader)
                    buffer = (op as DownLoader).data;
                if (op.isErr)
                {
                    bundle.SetErr(op.error);
                    End(null);
                    return;
                }


                buffer = EncryptBuffer.Decode(bundleName, buffer, encrypt);
                if (raw)
                {
                    rawObject = RawObject.Create(path, buffer);
                    End(null);
                }
                else
                {
                    if (async)
                    {
                        if (loadOp == null)
                            loadOp = AssetBundle.LoadFromMemoryAsync(buffer);
                        if (!loadOp.isDone) return;
                        End(loadOp.assetBundle);
                    }
                    else
                        End(AssetBundle.LoadFromMemory(buffer));
                }
            }

            public abstract void UnLoad();
        }
        private class FromFileMode : Mode
        {
            private ReadFileOperation readFileOperation;
            FileStream filestream;
            private AssetBundleCreateRequest loadOp;
            private bool fromBytes = false;
            public override float progress
            {
                get
                {
                    if (fromBytes)
                        return bytesProgress;
                    return loadOp.progress;
                }
            }

            public FromFileMode(Bundle bundle) : base(bundle)
            {
                if (raw || (compress != CompressType.LZMA && !(encrypt is NoneAssetStreamEncrypt)))
                {
                    fromBytes = true;
                    readFileOperation = AssetsHelper.ReadFile(path, async);
                }
                else
                {
                    filestream = new BundleStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bundleName, encrypt);
                    if (async)
                    {
                        loadOp = AssetBundle.LoadFromStreamAsync(filestream);
                    }
                    else
                        End(AssetBundle.LoadFromStream(filestream));
                }
            }

            protected override void OnUpdate()
            {
                if (fromBytes)
                {
                    LoadFromBytes(readFileOperation);
                }
                else
                {
                    if (async)
                    {
                        if (!loadOp.isDone) return;
                        End(loadOp.assetBundle);
                    }
                }
            }
            public override void UnLoad()
            {
                if (filestream != null)
                {
                    filestream.Dispose();
                    filestream = null;
                }
            }
        }
        private class FromRequestMode : Mode
        {
            private DownLoader downloader;

            private bool fromBytes = false;

            public FromRequestMode(Bundle bundle) : base(bundle)
            {
                if (raw || (!raw && !(encrypt is NoneAssetStreamEncrypt)))
                    fromBytes = true;
                this.downloader = AssetsInternal.DownLoadBundle(AssetsInternal.GetVersion(), bundleName, fromBytes);
            }

            public override float progress => fromBytes ? bytesProgress : this.downloader.progress;
            protected override void OnUpdate()
            {
                if (fromBytes)
                {
                    LoadFromBytes(this.downloader);
                }
                else
                {
                    if (!downloader.isDone) return;
                    End((downloader as BundleDownLoader).bundle);
                }
            }

            public override void UnLoad()
            {
            }
        }



        public override float progress => mode?.progress ?? 0;


        private long ProfilerAsset(AssetBundle value)
        {
            if (value == null) return 0;
            return Profiler.GetRuntimeMemorySizeLong(value);
        }

        protected void SetResult(AssetBundle value)
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
            this.value = value;
            InvokeComplete();
        }





        protected override void OnLoad()
        {
            if (mode == null)
                mode = type == BundleLoadType.FromRequest ? new FromRequestMode(this) : new FromFileMode(this);
            mode?.Update();
        }

        protected sealed override void OnUnLoad()
        {
            if (value != null)
                value.Unload(true);
            mode?.UnLoad();
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

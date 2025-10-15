using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace WooAsset
{

    public class Bundle : AssetOperation
    {
        public enum BundleLoadType
        {
            FromFile,
            FromRequest,
        }
        private bool _async = false;
        private string _path;
        private long _length;
        private BundleLoadType type;
        public long length => _length;
        public override bool async => _async;
        public readonly string bundleName;
        private readonly IAssetEncrypt encrypt;
        public readonly bool raw;
        public readonly long rawLength;
        private readonly CompressType compress;
        private readonly uint bundleCrc;
        private readonly Hash128 bundleHash;
        private readonly Operation dependence;


        private RawObject rawObject => mode.rawObject;

        public AssetBundle value { get; private set; }

        private Mode mode;

        public static BundleLoadType GetLoadType(string bundleName)
        {
            var type = AssetsInternal.GetBundleAlwaysFromWebRequest() ? BundleLoadType.FromRequest : BundleLoadType.FromFile;
            var _path = AssetsInternal.GetBundleLocalPath(bundleName);
            if (type == BundleLoadType.FromFile && !AssetsHelper.ExistsFile(_path))
            {
                type = BundleLoadType.FromRequest;
            }
            return type;
        }

        internal Bundle(BundleLoadArgs loadArgs)
        {
            bundleName = loadArgs.bundleName;
            encrypt = loadArgs.encrypt;
            raw = loadArgs.data.raw;
            rawLength = loadArgs.data.length;
            dependence = loadArgs.dependence;
            compress = loadArgs.data.compress;
            bundleCrc = loadArgs.data.bundleCrc;
            bundleHash = Hash128.Parse(loadArgs.data.bundleHash);
            _path = AssetsInternal.GetBundleLocalPath(bundleName);
            _async = loadArgs.async;
            type = GetLoadType(bundleName);
            if (type == BundleLoadType.FromRequest)
            {
                _async = true;
            }
        }

        private abstract class Mode
        {
            public virtual float progress
            {
                get
                {
                    if (bytes_op == null)
                        return _progress;
                    else
                    {
                        if (loadOp == null)
                            return bytes_op.progress * 0.5f;
                        return loadOp.progress * 0.5f + 0.5f;
                    }
                }
            }
            private Bundle bundle;
            public bool raw => bundle.raw;
            public IAssetEncrypt encrypt => bundle.encrypt;
            protected CompressType compress => bundle.compress;
            protected uint bundleCrc => bundle.bundleCrc;
            protected Hash128 bundleHash => bundle.bundleHash;
            public string path => bundle._path;
            public bool async => bundle._async;
            public string bundleName => bundle.bundleName;
            public Operation dependence => bundle.dependence;
            public RawObject rawObject { get; private set; }

            protected abstract float _progress { get; }

            private Operation bytes_op;
            private AssetBundleCreateRequest loadOp;
            protected Mode(Bundle bundle)
            {
                this.bundle = bundle;
            }

            public void Load()
            {
                var formBytesOperation = BeforeLoad();
                if (formBytesOperation != null)
                {
                    bytes_op = formBytesOperation;
                    LoadFromBytes(bytes_op);
                }
                else
                    OnLoad();

            }
            protected abstract Operation BeforeLoad();
            protected abstract void OnLoad();
            protected async void End(AssetBundle value)
            {
                await dependence;
                bundle.SetResult(value);
            }
            byte[] buff_release;
            private async void LoadFromBytes(Operation op)
            {

                await op;
                byte[] buffer = null;
                if (op is ReadFileOperation)
                {
                    buffer = (op as ReadFileOperation).bytes;
                }
                else if (op is DownLoader)
                {
                    var data = (op as BytesDownLoader).data;
                    buffer = AssetsHelper.AllocateByteArray(data.Length);
                    Array.Copy(data, buffer, data.Length);
                    buff_release = buffer;
                }



                if (op.isErr)
                {
                    bundle.SetErr(op.error);
                    End(null);
                    return;
                }


                if (bundle.type == BundleLoadType.FromRequest
                    && AssetsInternal.GetCachesDownloadedBundles()
                    && !AssetsInternal.GetBundleAlwaysFromWebRequest())
                    await AssetsHelper.WriteFile(buffer, path, 0, buffer.Length);


                buffer = encrypt.Decode(bundleName, buffer);

                if (raw)
                {
                    rawObject = RawObject.Create(path, buffer);
                    End(null);
                }
                else
                {
                    if (async)
                    {
                        loadOp = AssetBundle.LoadFromMemoryAsync(buffer, bundleCrc);
                        await loadOp;
                        End(loadOp.assetBundle);
                    }
                    else
                        End(AssetBundle.LoadFromMemory(buffer, bundleCrc));
                }
            }

            public virtual void UnLoad()
            {
                if (buff_release != null)
                {
                    AssetsHelper.RecycleByteArray(buff_release);
                    buff_release = null;
                }
            }
        }
        private class FromFileMode : Mode
        {
            FileStream filestream;
            private AssetBundleCreateRequest loadOp;
            protected override float _progress => async ? loadOp.progress : 0.9f;

            public FromFileMode(Bundle bundle) : base(bundle) { }

            protected override Operation BeforeLoad()
            {
                if (raw || (compress != CompressType.LZMA && !(encrypt is NoneAssetStreamEncrypt) && !(typeof(OffsetAssetStreamEncrypt).IsAssignableFrom(encrypt.GetType()))))
                {
                    return AssetsHelper.ReadFile(path, async);
                }
                else
                {
                    if (typeof(OffsetAssetStreamEncrypt).IsAssignableFrom(encrypt.GetType()))
                    {
                        var en = encrypt as OffsetAssetStreamEncrypt;
                        ulong offset = en.GetOffset(bundleName);
                        if (async)
                            loadOp = AssetBundle.LoadFromFileAsync(path, bundleCrc, offset);
                        else
                            End(AssetBundle.LoadFromFile(path, bundleCrc, offset));
                    }
                    else
                    {
                        filestream = new BundleStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bundleName, encrypt);
                        if (async)
                            loadOp = AssetBundle.LoadFromStreamAsync(filestream, bundleCrc);
                        else
                            End(AssetBundle.LoadFromStream(filestream, bundleCrc));
                    }
                    return null;
                }
            }
            protected override async void OnLoad()
            {
                if (async)
                {
                    await loadOp;
                    End(loadOp.assetBundle);
                }
            }


            public override void UnLoad()
            {
                base.UnLoad();
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


            public FromRequestMode(Bundle bundle) : base(bundle)
            {

            }

            protected override float _progress => this.downloader.progress;
            protected override Operation BeforeLoad()
            {
                var fromBytes = false;
                if (raw || (!raw && !(encrypt is NoneAssetStreamEncrypt)))
                    fromBytes = true;

                if (fromBytes)
                    this.downloader = AssetsInternal.DownLoadRawBundle(AssetsInternal.GetVersion(), bundleName);
                else
                    this.downloader = AssetsInternal.DownLoadBundle(AssetsInternal.GetVersion(), bundleName, bundleCrc, bundleHash);
                return fromBytes ? this.downloader : null;
            }

            protected override async void OnLoad()
            {
                await downloader;
                End((downloader as BundleDownLoader).bundle);
            }




        }



        public override float progress => mode?.progress ?? 0;


        protected virtual long ProfileAsset(AssetBundle value)
        {

            if (raw) return rawLength;
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



            _length = ProfileAsset(value);
            this.value = value;
            InvokeComplete();
        }
    




        protected override void OnLoad()
        {
            mode = type == BundleLoadType.FromRequest ?
                new FromRequestMode(this) : new FromFileMode(this);
            mode?.Load();
        }

        protected sealed override void OnUnLoad()
        {
            if (value != null)
                value.Unload(true);
            mode?.UnLoad();
        }

        public virtual RawObject LoadRawObject(string path) => rawObject;
        internal virtual AssetRequest LoadAssetWithSubAssetsAsync(string name, Type type) => RuntimeAssetRequest.Request(value.LoadAssetWithSubAssetsAsync(name, type));
        public virtual UnityEngine.Object[] LoadAssetWithSubAssets(string name, Type type) => value.LoadAssetWithSubAssets(name, type);
        internal virtual AssetRequest LoadAssetAsync(string name, Type type) => RuntimeAssetRequest.Request(value.LoadAssetAsync(name, type));
        public virtual UnityEngine.Object LoadAsset(string name, Type type) => value.LoadAsset(name, type);
        public virtual Scene LoadScene(string path, LoadSceneParameters parameters) => SceneManager.LoadScene(AssetsHelper.GetFileNameWithoutExtension(path), parameters);
        public AsyncOperation UnloadSceneAsync(string path, UnloadSceneOptions op) => SceneManager.UnloadSceneAsync(AssetsHelper.GetFileNameWithoutExtension(path), op);
        public virtual AsyncOperation LoadSceneAsync(string path, LoadSceneParameters parameters) => SceneManager.LoadSceneAsync(AssetsHelper.GetFileNameWithoutExtension(path), parameters);
    }

}

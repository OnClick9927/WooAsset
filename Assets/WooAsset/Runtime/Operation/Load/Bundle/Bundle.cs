using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace WooAsset
{
    public class BundleStream : FileStream
    {

        private readonly string bundleName;
        private IAssetStreamEncrypt encrypt;
        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share, string bundleName, IAssetStreamEncrypt encrypt) : base(path, mode, access, share)
        {
            this.bundleName = bundleName;
            this.encrypt = encrypt;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            EncryptBuffer.Decode(bundleName, array, offset, count, encrypt);

            return index;
        }
    }

    public struct BundleLoadArgs : IAssetArgs
    {
        public string bundleName;
        public bool async;
        public IAssetStreamEncrypt encrypt;
        public BundleLoadArgs(string bundleName, bool async, IAssetStreamEncrypt en)
        {
            this.bundleName = bundleName;
            this.async = async;
            this.encrypt = en;
        }
    }
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

        protected override long ProfilerAsset(AssetBundle value)
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
        private async void LoadFromStream()
        {
            BundleStream bs = new BundleStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, bundleName,encrypt);
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
                    var downloader = AssetsInternal.DownLoadBundleBytes(bundleName);
                    this.downloader = downloader;
                    await downloader;
                    if (!downloader.isErr)
                    {
                        byte[] buffer = downloader.data;
                        await downloader.SaveBundleToLocal();
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

        public AssetBundleRequest LoadAssetAsync(string name, Type type)
        {
            return value.LoadAssetWithSubAssetsAsync(name, type);
        }
        public UnityEngine.Object[] LoadAsset(string name, Type type)
        {
            return value.LoadAssetWithSubAssets(name, type);
        }

    }
}

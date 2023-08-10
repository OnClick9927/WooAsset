using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace WooAsset
{
    public struct BundleLoadArgs : IAssetArgs
    {
        public string bundleName;
        public bool async;
        public BundleLoadArgs(string bundleName, bool async)
        {
            this.bundleName = bundleName;
            this.async = async;
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
        public override bool async => _async;
        protected BundleLoadArgs loadArgs;
        public string bundleName => loadArgs.bundleName;

        private AssetBundleCreateRequest loadOp;
        private BundleDownloader downloader;
        public Bundle(BundleLoadArgs loadArgs)
        {
            this.loadArgs = loadArgs;
            _async = loadArgs.async;
            type = BundleLoadType.FromFile;
            _path = AssetsInternal.GetBundleLocalPath(bundleName);
            if (!AssetsHelper.ExistsFile(_path))
                type = BundleLoadType.FromRequest;
            _async = type == BundleLoadType.FromRequest;

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
            buffer = EncryptBuffer.Decode(loadArgs.bundleName, buffer, AssetsInternal.GetEncrypt());
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
        protected override async void OnLoad()
        {
            if (type == BundleLoadType.FromFile)
            {
                var reader = await AssetsHelper.ReadFile(_path, async);
                LoadBundle(reader.bytes);
            }
            else
            {
                downloader = AssetsInternal.DownLoadBundle(bundleName);
                await downloader;
                if (!downloader.isErr)
                {
                    byte[] buffer = downloader.data;
                    if (AssetsInternal.GetSaveBundlesWhenPlaying())
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

using System;
using System.IO;
using UnityEngine;

namespace WooAsset
{
    public class Bundle : AssetHandle<AssetBundle>
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
            if (!File.Exists(_path))
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
                LoadBundle(File.ReadAllBytes(_path));
            }
            else
            {
                downloader = AssetsInternal.DownLoadBundle(bundleName);
                await downloader;
                if (!downloader.isErr)
                {
                    byte[] buffer = downloader.data;
                    if (AssetsInternal.GetSaveBundlesWhenPlaying()) 
                        downloader.SaveBundleToLocal();
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

        protected sealed override void OnRetain(bool old) { }
    }
}

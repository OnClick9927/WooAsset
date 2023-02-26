using System;
using System.IO;
using UnityEngine;
using static WooAsset.AssetsInternal;

namespace WooAsset
{
    public class Bundle : Asset<AssetBundle>
    {
        protected BundleLoadArgs loadArgs;
        private AssetBundleCreateRequest loadOp;
        public Bundle(BundleLoadArgs loadArgs)
        {
            this.loadArgs = loadArgs;
        }

        public override float progress
        {
            get
            {
                return isDone ? 1 : (loadOp == null) ? 0 : loadOp.progress;
            }
        }

        public override string path => loadArgs.path;

        protected override async void OnLoad()
        {
            AssetEncryptStream fileStream = new AssetEncryptStream(loadArgs.bundleName, loadArgs.path, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 4, false);
            loadOp = AssetBundle.LoadFromStreamAsync(fileStream);
            await this.loadOp;
            fileStream.Dispose();
            SetResult(loadOp.assetBundle);
        }

        protected override void OnUnLoad()
        {
            value.Unload(true);
            Resources.UnloadUnusedAssets();
        }

        public AssetBundleRequest LoadAssetAsync(string name, Type type)
        {
            return value.LoadAssetAsync(name, type);
        }
    }
}

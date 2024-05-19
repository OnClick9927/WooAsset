using System;
using static WooAsset.ManifestData;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public abstract class AssetHandle : AssetOperation<Object>
    {
        protected Type type => loadArgs.type;

        public override bool async => loadArgs.async;
        protected Bundle bundle { get; private set; }
        protected AssetData data => loadArgs.data;

        public AssetType assetType => data.type;
        public virtual string path => data.path;
        public string bundleName => data.bundleName;
        protected bool direct => loadArgs.direct;
        private AssetLoadArgs loadArgs;
        protected Bundle LoadBundle()
        {
            if (string.IsNullOrEmpty(bundleName))
                return null;
            bundle = AssetsInternal.LoadBundle(bundleName, async);
            return bundle;
        }
        public AssetHandle(AssetLoadArgs loadArgs)
        {
            this.loadArgs = loadArgs;
        }
        protected sealed override void OnUnLoad() { }
        protected sealed override async void OnLoad()
        {
            if (AssetsLoop.isBusy)
                await new WaitBusyOperation();
            InternalLoad();
        }

        protected abstract void InternalLoad();

    }

}

using System;
using static WooAsset.ManifestData;
namespace WooAsset
{
    public abstract class AssetHandle<T> : AssetHandle
    {
        protected AssetHandle(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {
        }

        public T value { get; private set; }
        protected virtual void SetResult(T value)
        {
            this.value = value;
            InvokeComplete();
        }
    }
    public abstract class AssetHandle : AssetOperation
    {
        protected Type type => loadArgs.type;

        public override bool async => loadArgs.async;
        protected Bundle bundle { get; private set; }
        public AssetData data => loadArgs.data;

        public AssetType assetType => data.type;
        public virtual string path => data.path;
        public string bundleName => data.bundleName;
        private AssetLoadArgs loadArgs;

        public AssetHandle(AssetLoadArgs loadArgs, Bundle bundle)
        {
            this.loadArgs = loadArgs;
            this.bundle = bundle;
        }
        protected sealed override void OnUnLoad() { }
        protected sealed override async void OnLoad()
        {
            if (AssetsLoop.isBusy)
                await new WaitBusyOperation();
            await bundle;
            InternalLoad();
        }

        protected abstract void InternalLoad();

    }

}

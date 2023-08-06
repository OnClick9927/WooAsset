using System.Collections.Generic;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public abstract class AssetHandle : AssetHandle<Object>
    {
        public override bool async => loadArgs.async;
        public bool isBundleUnloaded => bundle == null ? false : bundle.unloaded;
        public Bundle bundle { get; private set; }
        public virtual string path => loadArgs.path;
        protected bool direct => loadArgs.direct;

        public string bundleName => loadArgs.bundleName;
        public List<AssetHandle> dps => loadArgs.dps;
        protected float dpProgress
        {
            get
            {
                if (dps == null || dps.Count == 0)
                    return 1;
                float dpSum = 0;
                for (int i = 0; i < dps.Count; i++)
                    dpSum += dps[i].progress / dps.Count;
                return dpSum;
            }
        }

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

        protected override sealed async void SetResult(Object value)
        {
            if (dps != null)
            {
                for (int i = 0; i < dps.Count; i++)
                {
                    var asset = dps[i];
                    if (asset.isDone) continue;
                    await asset;
                }
            }
            base.SetResult(value);
        }
        protected sealed override void OnUnLoad() { }
        protected override sealed long ProfilerAsset(Object value)
        {
            return value == null ? 0 : Profiler.GetRuntimeMemorySizeLong(value);
        }
    }

}

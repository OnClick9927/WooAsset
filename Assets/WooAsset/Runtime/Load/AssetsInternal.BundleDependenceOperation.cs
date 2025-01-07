using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private class BundleDependenceOperation : GroupOperation<Bundle>
        {
            public override float progress => isDone ? 1 : _progress;
            private float _progress = 0;
            public BundleDependenceOperation(BundleData bundleData, bool async)
            {
                var dps = bundleData.dependence;
                if (dps == null)
                   InvokeComplete();
                else
                {
                    List<Bundle> _bundles = new List<Bundle>();
                    for (int i = 0; i < dps.Count; i++)
                    {
                        var bundle = bundles.LoadBundle(dps[i], async);
                        _bundles.Add(bundle);
                    }
                    base.Done(_bundles);
                }

            }
        }
    }
}

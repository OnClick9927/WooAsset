using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private class BundleDependenceOperation : Operation
        {
            public override float progress => isDone ? 1 : _progress;
            private float _progress = 0;
            private int _index = 0;
            private int _count;
            private List<Bundle> _bundles = new List<Bundle>();
            public BundleDependenceOperation(BundleData bundleData, bool async)
            {
                Done(bundleData, async);
            }
            private void Done(BundleData bundleData, bool async)
            {

                var dps = bundleData.dependence;

                if (dps != null)
                {
                    _count = dps.Count;
                    for (int i = 0; i < dps.Count; i++)
                    {
                        var bundle = bundles.LoadBundle(dps[i], async);
                        _bundles.Add(bundle);

                        bundle.completed += Bundle_completed;
                    }
                }
                else
                {
                    InvokeComplete();

                }
            }

            private void Bundle_completed()
            {
                _index++;
                _progress = (float)_index / _count;

                if (_index >= _count)
                {
                    for (int i = 0; i < _bundles.Count; i++)
                        _bundles[i].completed -= Bundle_completed;
                    InvokeComplete();
                }
            }
        }
    }
}

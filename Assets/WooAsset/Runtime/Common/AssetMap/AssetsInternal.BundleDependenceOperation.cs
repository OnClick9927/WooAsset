namespace WooAsset
{
    partial class AssetsInternal
    {
        public class BundleDependenceOperation : Operation
        {
            public override float progress => isDone ? 1 : _progress;
            private float _progress = 0;
            public BundleDependenceOperation(BundleData bundleData, bool async)
            {
                Done(bundleData, async);
            }
            private async void Done(BundleData bundleData, bool async)
            {
                var dps = bundleData.dependence;

                if (dps != null)
                {
                    for (int i = 0; i < dps.Count; i++)
                    {
                        await bundles.LoadBundle(dps[i], async);
                        _progress = (float)i / dps.Count;
                    }
                }
                InvokeComplete();
            }

        }
    }
}

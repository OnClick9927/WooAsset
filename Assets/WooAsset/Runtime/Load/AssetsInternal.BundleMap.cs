using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private class BundleMap : NameMap<Bundle, BundleLoadArgs>
        {
            protected override Bundle CreateNew(BundleLoadArgs args) => AssetsInternal.CreateBundle((BundleLoadArgs)args);

            public Bundle LoadBundle(string bundleName, bool async)
            {
                var data = GetBundleData(bundleName);
                BundleLoadArgs args = default;
                args = new BundleLoadArgs(data, async, GetEncrypt(data.enCode));
                return LoadAsync(args);
            }
            protected override void BeforeLoad(bool create, ref BundleLoadArgs args)
            {

                var dps = args.data.dependence;
                var async = args.async;
                if (create)
                {
                    if (dps == null || dps.Count == 0)
                        args.dependence = Operation.empty;
                    else
                    {
                        List<Bundle> _bundles = new List<Bundle>();
                        for (int i = 0; i < dps.Count; i++)
                            _bundles.Add(bundles.LoadBundle(dps[i], async));
                        var op = new GroupOperation<Bundle>();
                        op.Done(_bundles);
                        args.dependence = op;
                    }
                }
                else
                {
                    if (dps == null || dps.Count == 0) return;
                    for (int i = 0; i < dps.Count; i++)
                        bundles.LoadBundle(dps[i], async);
                }
            }


            public override void Release(string uid)
            {
                Bundle result = Find(uid);
                if (result == null) return;

                ReleaseRef(result);
                if (GetAutoUnloadBundle())
                    TryRealUnload(uid);
                var bundleData = GetBundleData(uid);
                var dp = bundleData.dependence;
                if (dp != null)
                    foreach (var item in dp)
                        Release(item);
            }
            private List<string> useless = new List<string>();
            public void UnloadBundles()
            {
                if (GetAutoUnloadBundle()) return;
                GetZeroRefKeys(useless);
                for (int i = 0; i < useless.Count; i++)
                {
                    TryRealUnload(useless[i]);
                }
            }



        }
    }
}

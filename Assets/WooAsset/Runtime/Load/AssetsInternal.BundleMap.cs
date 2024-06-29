using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private class BundleMap : NameMap<Bundle>
        {
            protected override Bundle CreateNew(IAssetArgs args) => AssetsInternal.CreateBundle((BundleLoadArgs)args);

            public Bundle LoadBundle(string bundleName, bool async)
            {
                var data = GetBundleData(bundleName);
                BundleDependenceOperation dps;
                dps = new BundleDependenceOperation(data, async);
                return LoadAsync(new BundleLoadArgs(data, async, GetEncrypt(data.enCode), dps));
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

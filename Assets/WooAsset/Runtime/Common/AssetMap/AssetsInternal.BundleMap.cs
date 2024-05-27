

using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private class BundleMap : NameMap<Bundle>
        {
            protected override Bundle CreateNew(IAssetArgs args) => AssetsInternal.CreateBundle((BundleLoadArgs)args);

            public override void Release(string uid)
            {
                Bundle result = Find(uid);
                if (result == null) return;
                var dp = result.dependence;
                if (dp != null)
                    foreach (var item in dp)
                        Release(item.bundleName);
                ReleaseRef(result);
                if (!GetAutoUnloadBundle()) return;
                TryRealUnload(uid);
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

            protected override void OnRetain(Bundle bundle, bool old)
            {
                RetainRef(bundle);
                if (!old) return;
                var dp = bundle.dependence;
                if (dp != null)
                    foreach (var item in dp)
                        RetainRef(item);
            }
        }
    }
}



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

            protected override void OnRetain(Bundle asset, bool old)
            {
                RetainRef(asset);
            }
        }
    }
}



using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsInternal
    {
        private class BundleMap : NameMap<Bundle>
        {
            public Bundle LoadAsync(BundleLoadArgs args) => base.LoadAsync(args.bundleName, args);
            protected override Bundle CreateNew(string path, IAssetArgs args) => new Bundle((BundleLoadArgs)args);

            public override void Release(string path)
            {
                Bundle result = Find(path);
                if (result == null) return;
                ReleaseRef(result);
                if (!GetAutoUnloadBundle()) return;
                TryRealUnload(path);
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
